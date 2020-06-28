using System;

namespace Orbit.Util
{
    public interface IDataGenerator
    {
        void Start();

        void Stop();

        event EventHandler? Started;

        event EventHandler? Stopped;
    }
}