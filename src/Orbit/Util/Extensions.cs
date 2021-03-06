﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace Orbit.Util
{
    public static class Extensions
    {
        internal static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext? context) => new SynchronizationContextAwaiter(context);
    }
    /// <summary>
    /// Helper struct used to cleanly switch between contexts to simplify EventMonitor-related code
    /// </summary>
    internal struct SynchronizationContextAwaiter : INotifyCompletion
    {
        private static readonly SendOrPostCallback _postCallback = state => ((Action)state)();

        private readonly SynchronizationContext? _context;
        public SynchronizationContextAwaiter(SynchronizationContext? context) => _context = context;

        public bool IsCompleted => _context == SynchronizationContext.Current;

        public void OnCompleted(Action continuation) => _context?.Post(_postCallback, continuation);

        public void GetResult() { }
    }
}
