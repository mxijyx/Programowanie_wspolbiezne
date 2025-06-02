//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall, IDisposable
    {
        #region Pola

        // Prywatne pole prędkości; nikt poza tą klasą nie może go zmienić
        private Vector _velocity;

        // Bufor diagnostyczny w pamięci:
        private readonly List<string> _diagnosticBuffer = new List<string>();

        // Wątek odpowiedzialny za pętlę ruchu:
        private readonly Thread _ballThread;
        private volatile bool _isRunning = true;

        #endregion

        #region Konstruktor

        internal Ball(Vector initialPosition, Vector initialVelocity, double mass, double diameter)
        {
            Position = initialPosition;
            _velocity = initialVelocity;
            Mass = mass;
            Diameter = diameter;

            _ballThread = new Thread(ThreadLoop)
            {
                Name = $"BallThread_{GetHashCode():X}",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            _ballThread.Start();
        }

        #endregion

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        // Zwracamy zawsze kopię wektora prędkości. Nikt z zewnątrz nie może zmienić oryginału.
        public IVector Velocity => new Vector(_velocity.x, _velocity.y);

        // Pozycja jest ustawiana wewnętrznie – nikt poza tą klasą (lub wewnętrznie) nie zmienia jej
        public IVector Position { get; internal set; }

        public double Mass { get; }
        public double Diameter { get; }

        #endregion

        #region Pętla ruchu (Real‐Time Loop)

        private void ThreadLoop()
        {
            while (_isRunning)
            {
                // Oblicz interwał (raz na iterację):
                int sleepMs = RefreshTimeCalculator();

                // Zaktualizuj pozycję + zbierz dane diagnostyczne:
                Move(sleepMs);

                // Poczekaj dokładnie tyle, ile wyliczyliśmy
                Thread.Sleep(sleepMs);
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }

        #endregion

        #region Metoda Move + diagnostyka

        private void Move(int refreshTime)
        {
            // 1) Pobierz aktualny czas
            DateTime now = DateTime.UtcNow;

            // 2) Zaktualizuj pozycję (korzystając z prywatnego _velocity)
            Position = new Vector(
                Position.x + (_velocity.x * refreshTime / 1000.0),
                Position.y + (_velocity.y * refreshTime / 1000.0)
            );

            // 3) Dodaj wiersz do bufora diagnostycznego (tylko pamięć):
            string line = $"{now:O}; " +
                          $"Pos=({Position.x:F2},{Position.y:F2}); " +
                          $"Vel=({_velocity.x:F2},{_velocity.y:F2}); " +
                          $"Δt={refreshTime}ms";
            lock (_diagnosticBuffer)
            {
                _diagnosticBuffer.Add(line);
            }

            // 4) Powiadom Warstwę Logiki (lub wyższą warstwę) o nowej pozycji:
            NewPositionNotification?.Invoke(this, Position);
        }

        #endregion

        #region RefreshTimeCalculator

        private int RefreshTimeCalculator()
        {
            // Oblicz długość wektora prędkości (w jednostkach/s)
            double actualVelocity = Math.Sqrt(_velocity.x * _velocity.x + _velocity.y * _velocity.y);

            const int maxRefreshTime = 100;
            const int minRefreshTime = 10;
            double normalizedVelocity = Math.Clamp(actualVelocity, 0.0, 1.0);

            return Math.Clamp(
                (int)(maxRefreshTime - normalizedVelocity * (maxRefreshTime - minRefreshTime)),
                minRefreshTime,
                maxRefreshTime
            );
        }

        #endregion

        #region Metoda do zmiany prędkości (tylko Data Layer + przyjaciele/im internal)

        /// <summary>
        ///   Umożliwia Warstwie Logiki (lub innym klasom w tej samej bibliotece)
        ///   jednorazową zmianę prędkości kuli. 
        /// </summary>
        internal void SetVelocity(Vector newVelocity)
        {
            _velocity = newVelocity;
        }

        #endregion

        #region Zapis diagnostyki do pliku

        /// <summary>
        ///   Zapisuje cały bufor w pamięci na dysk i czyści go. 
        ///   Każdy wywołujący powinien przekazać ścieżkę do katalogu, np. ".../logs/".
        /// </summary>
        public void SaveDiagnosticsToFile(string folderPath)
        {
            List<string> snapshot;
            lock (_diagnosticBuffer)
            {
                snapshot = new List<string>(_diagnosticBuffer);
                _diagnosticBuffer.Clear();
            }

            Directory.CreateDirectory(folderPath);
            string fileName = $"ball_{GetHashCode():X}_{DateTime.Now:yyyyMMdd_HHmmss}.log";
            string fullPath = Path.Combine(folderPath, fileName);

            File.WriteAllLines(fullPath, snapshot);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            // 1) Zatrzymaj pętlę ruchu
            Stop();

            // 2) Poczekaj, aż wątek zakończy ostatnią iterację
            if (!_ballThread.Join(TimeSpan.FromSeconds(3)))
            {
                // opcjonalnie: Debug.WriteLine("Wątek kuli nie zakończył się w terminie");
            }

            // 3) Wypisz zgromadzone dane diagnostyczne do osobnego pliku
            string logsFolder = Path.Combine(
                Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)!,
                "logs"
            );
            SaveDiagnosticsToFile(logsFolder);
        }

        #endregion
    }
}