using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Data.Models;

namespace Data.Journal
{
    // High-throughput journal processor using a producer-consumer Channel and batched EF inserts.
    // - Producer reads lines from a stream and writes them to the channel.
    // - A fixed number of worker tasks read lines, parse the event token with Utf8JsonReader,
    //   deserialize into the mapped model, and accumulate batches.
    // - Each worker uses its own DbContext scope (create per batch or per worker) and
    //   calls AddRange/SaveChanges in batches to reduce DB round-trips.
    public static class JournalProcessor
    {
        // Tune these for your environment
        private const int DefaultWorkerCount = 4;
        private const int DefaultBatchSize = 500;
        private const int ChannelCapacity = 10000;

        public static async Task ProcessFileAsync(string path, Func<List<object>, Task> storeBatchAsync,
            Func<List<object>, Exception, string, Task>? handleFailedBatch = null,
            int workerCount = DefaultWorkerCount, int batchSize = DefaultBatchSize,
            CancellationToken cancellation = default)
        {
            var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(ChannelCapacity)
            {
                SingleWriter = true,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.Wait
            });

            // Start workers
            var workers = new List<Task>(workerCount);
            for (int i = 0; i < workerCount; i++)
            {
                // pass source file path into workers so failure handler can know origin
                workers.Add(Task.Run(() => WorkerAsync(channel.Reader, storeBatchAsync, handleFailedBatch, path, batchSize, cancellation), cancellation));
            }

            // Producer: read file line-by-line and write into channel
            await ProduceLinesAsync(path, channel.Writer, cancellation).ConfigureAwait(false);

            // Signal completion and wait for workers
            channel.Writer.Complete();
            await Task.WhenAll(workers).ConfigureAwait(false);
        }

        private static async Task ProduceLinesAsync(string path, ChannelWriter<string> writer, CancellationToken cancellation)
        {
            // Use FileStream + StreamReader to avoid reading entire file into memory
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16, FileOptions.SequentialScan);
            using var sr = new StreamReader(fs);

            string? line;
            while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                cancellation.ThrowIfCancellationRequested();
                // Wait when channel is full
                await writer.WriteAsync(line, cancellation).ConfigureAwait(false);
            }
        }

        private static async Task WorkerAsync(ChannelReader<string> reader, Func<List<object>, Task> storeBatchAsync, Func<List<object>, Exception, string, Task>? handleFailedBatch, string sourceFile, int batchSize, CancellationToken cancellation)
        {
            // Local batch list for models to be stored
            var batch = new List<object>(batchSize);

            await foreach (var line in reader.ReadAllAsync(cancellation))
            {
                cancellation.ThrowIfCancellationRequested();

                var model = DeserializeMinimal(line);
                if (model != null)
                {
                    batch.Add(model);
                }

                if (batch.Count >= batchSize)
                {
                    await FlushBatchAsync(batch, storeBatchAsync, handleFailedBatch, sourceFile, cancellation).ConfigureAwait(false);
                }
            }

            // flush remaining
            if (batch.Count > 0)
            {
                await FlushBatchAsync(batch, storeBatchAsync, handleFailedBatch, sourceFile, cancellation).ConfigureAwait(false);
            }
        }

        private static async Task FlushBatchAsync(List<object> batch, Func<List<object>, Task> storeBatchAsync, Func<List<object>, Exception, string, Task>? handleFailedBatch, string sourceFile, CancellationToken cancellation)
        {
            try
            {
                cancellation.ThrowIfCancellationRequested();
                // Pass the whole batch to the provided batch store callback
                await storeBatchAsync(batch).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // If a handler is provided, call it to persist the failed batch and continue
                if (handleFailedBatch != null)
                {
                    try
                    {
                        await handleFailedBatch(batch, ex, sourceFile).ConfigureAwait(false);
                    }
                    catch
                    {
                        // swallow exceptions from the failure handler to avoid stopping the import
                    }
                }
            }
            finally
            {
                batch.Clear();
            }
        }

        // Minimal deserialization: read 'event' property quickly using Utf8JsonReader and dispatch to mapped types.
        // For FSDJump we map StarPos array to properties; FSSAllBodiesFound uses default mapping.
        private static object? DeserializeMinimal(string json)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes, isFinalBlock: true, state: default);

            string? eventName = null;
            // Scan for "event" property at root
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (string.Equals(propName, "event", StringComparison.OrdinalIgnoreCase))
                    {
                        reader.Read();
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            eventName = reader.GetString();
                        }
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(eventName)) return null;

            switch (eventName)
            {
                case "FSDJump":
                    // We need to handle StarPos array mapping — deserialize into DTO then map
                    try
                    {
                        var dto = JsonSerializer.Deserialize<FSDJumpDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (dto == null) return null;

                        var model = new FSDJump
                        {
                            Timestamp = dto.Timestamp,
                            Event = dto.Event ?? string.Empty,
                            StarSystem = dto.StarSystem ?? string.Empty,
                            SystemAddress = dto.SystemAddress,
                            StarPosX = dto.StarPos?.Length > 0 ? dto.StarPos[0] : (double?)null,
                            StarPosY = dto.StarPos?.Length > 1 ? dto.StarPos[1] : (double?)null,
                            StarPosZ = dto.StarPos?.Length > 2 ? dto.StarPos[2] : (double?)null,
                            JumpDist = dto.JumpDist,
                            FuelUsed = dto.FuelUsed,
                            FuelLevel = dto.FuelLevel,
                            StarClass = dto.StarClass,
                            RawJson = json
                        };
                        return model;
                    }
                    catch
                    {
                        return null;
                    }

                case "FSSAllBodiesFound":
                    try
                    {
                        var model = JsonSerializer.Deserialize<FSSAllBodiesFound>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (model != null)
                        {
                            model.RawJson = json;
                        }
                        return model;
                    }
                    catch
                    {
                        return null;
                    }

                default:
                    return null;
            }
        }

        private class FSDJumpDto
        {
            public DateTime Timestamp { get; set; }
            public string? Event { get; set; }
            public string? StarSystem { get; set; }
            public long? SystemAddress { get; set; }
            public double[]? StarPos { get; set; }
            public double? JumpDist { get; set; }
            public double? FuelUsed { get; set; }
            public double? FuelLevel { get; set; }
            public string? StarClass { get; set; }
        }
    }
}
