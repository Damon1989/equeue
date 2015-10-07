﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using EQueue.Broker.Storage;
using ECommon.Utilities;

namespace EQueue.Broker.DeleteMessageStrategies
{
    public class DeleteMessageByTimeStrategy : IDeleteMessageStrategy
    {
        /// <summary>表示消息可以保存的最大小时数；
        /// <remarks>
        /// 比如设置为24 * 7，则表示如果某个chunk里的所有消息都消费过了，且该chunk里的所有消息都是24 * 7小时之前存储的，则该chunk就可以被删除了。
        /// 默认值为24 * 30，即保存一个月；用户可以根据自己服务器磁盘的大小决定消息可以保留多久。
        /// </remarks>
        /// </summary>
        public int MaxStorageHours { get; private set; }

        public DeleteMessageByTimeStrategy(int maxStorageHours = 24 * 30)
        {
            Ensure.Positive(maxStorageHours, "maxStorageHours");
            MaxStorageHours = maxStorageHours;
        }

        public IEnumerable<TFChunk> GetAllowDeleteChunks(TFChunkManager chunkManager, long maxMessagePosition)
        {
            var chunks = new List<TFChunk>();
            var allCompletedChunks = chunkManager
                .GetAllChunks()
                .Where(x => x.IsCompleted && x.ChunkHeader.ChunkDataEndPosition <= maxMessagePosition);

            foreach (var chunk in allCompletedChunks)
            {
                var lastWriteTime = new FileInfo(chunk.FileName).LastWriteTime;
                var storageHours = (DateTime.Now - lastWriteTime).TotalHours;
                if (storageHours >= MaxStorageHours)
                {
                    chunks.Add(chunk);
                }
            }

            return chunks;
        }
    }
}