using NetCord;
using NetCord.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoelhoBot.Bot.Memory
{
    public static class MemoryTaskHelper
    {
        public static CancellationTokenSource? cancellationTokenSource;
        public static void StopLoop()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
        }
        public static void ResetLoop()
        {
            StopLoop();
            if (cancellationTokenSource == null) return;
            _ = MemoryManager.LoopReply(cancellationTokenSource.Token);
        }
    }
}
