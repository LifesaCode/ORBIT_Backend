using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Orbit.Data;
using Orbit.Models;

namespace Orbit.Util
{
    public sealed class DataGenerator : IDataGenerator
    {
        private static readonly Lazy<IDataGenerator> _instance = new Lazy<IDataGenerator>(() => new DataGenerator());
        public static IDataGenerator Instance => _instance.Value;

        private DataGenerator()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => this._cancellationTokenSource.Cancel();
        }

        private Thread? _eventThread;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private IServiceProvider ServiceProvider => OrbitServiceProvider.Instance;

        public void Start()
        {
            if (_eventThread == null)
            {
                _eventThread = new Thread(SimulateDataGeneration);
                _eventThread.Start();
            }
        }

        public void Stop()
        {
            if (!this._cancellationTokenSource.IsCancellationRequested)
            {
                this._cancellationTokenSource.Cancel();
            }
        }

        public event EventHandler? Started;

        public event EventHandler? Stopped;

        /// <summary>
        /// This method simulates generating real-world data and inserting it into the database.
        /// </summary>
        /// <returns> </returns>
        private void SimulateDataGeneration()
        {
            CancellationToken token = _cancellationTokenSource.Token;

            this.Started?.Invoke(this, EventArgs.Empty);
            bool isFirst = true;
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));

                if (token.IsCancellationRequested)
                {
                    break;
                }

                using var scope = ServiceProvider.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<OrbitDbContext>();

                if (isFirst)
                {
                    db.InsertSeedData();
                    isFirst = false;
                    db.SaveChanges();
                    continue;
                }
                
                #region Get data from context

                WasteWaterStorageTankData wasteTank = db.WasteWaterStorageTankData.Last();
                UrineSystemData urineSystem = db.UrineProcessorData.Last();
                WaterProcessorData waterProcessor = db.WaterProcessorData.Last();
                PowerSystemData power = db.PowerSystemData.Last();
                AtmosphereData atmo = db.AtmosphereData.Last();
                CarbonDioxideRemediation co2 = db.CarbonDioxideRemoverData.Last();
                ExternalCoolantLoopData eloop = db.ExternalCoolantLoopData.Last();
                InternalCoolantLoopData iloop = db.InternalCoolantLoopData.Last();
                OxygenGenerator o2 = db.OxygenGeneratorData.Last();
                WaterGeneratorData h20 = db.WaterGeneratorData.Last();

                #endregion Get data from context


                #region Process the data

                urineSystem.ProcessData(wasteTank.Level);
                wasteTank.ProcessData(urineSystem.Status, waterProcessor.SystemStatus);
                waterProcessor.ProcessData(wasteTank.Level);
                power.ProcessData();
                atmo.ProcessData();
                co2.ProcessData();
                eloop.ProcessData();
                iloop.ProcessData();
                o2.ProcessData();
                h20.ProcessData();

                #endregion Process the data


                #region Generate new data sets

                var nextUrineSystem = new UrineSystemData(urineSystem);
                var nextWasteTank = new WasteWaterStorageTankData { Level = wasteTank.Level };
                var nextWaterProcessor = new WaterProcessorData(waterProcessor);
                var nextPower = new PowerSystemData(power);
                var nextAtmo = new AtmosphereData(atmo);
                var nextCo2 = new CarbonDioxideRemediation(co2);
                var nextEloop = new ExternalCoolantLoopData(eloop);
                var nextIloop = new InternalCoolantLoopData(iloop);
                var nextO2 = new OxygenGenerator(o2);
                var nextH20 = new WaterGeneratorData(h20);

                #endregion Generate new data sets


                #region Save new data sets to context

                db.UrineProcessorData.Add(nextUrineSystem);
                db.WasteWaterStorageTankData.Add(nextWasteTank);
                db.WaterProcessorData.Add(nextWaterProcessor);
                db.PowerSystemData.Add(nextPower);
                db.AtmosphereData.Add(nextAtmo);
                db.CarbonDioxideRemoverData.Add(nextCo2);
                db.ExternalCoolantLoopData.Add(nextEloop);
                db.InternalCoolantLoopData.Add(nextIloop);
                db.OxygenGeneratorData.Add(nextO2);
                db.WaterGeneratorData.Add(nextH20);
               
                db.SaveChanges();

                #endregion Save new data sets to context
            }

            this.Stopped?.Invoke(this, EventArgs.Empty);
        }
    }
}
