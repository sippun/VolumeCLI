using System;
using System.Diagnostics;
using NAudio.CoreAudioApi;

class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            ShowSessions();

            Console.WriteLine("\nPress a key:");
            Console.WriteLine("  [m] Mute | [u] Unmute | [s] Set Volume | [r] Refresh | [q] Quit");

            var key = Console.ReadKey(intercept: true).Key;

            switch (key)
            {
                case ConsoleKey.M:
                    MuteOrUnmute();
                    break;
                case ConsoleKey.S:
                    SetVolume();
                    break;
                case ConsoleKey.R:
                    continue; // Just refresh the screen
                case ConsoleKey.Q:
                    return; // Exit program
                default:
                    Console.WriteLine("Invalid key.");
                    break;
            }
        }
    }

    static void ShowSessions()
    {
        Console.Clear();
        Console.WriteLine("Running audio sessions:\n");

        var sessions = GetCurrentSessions();

        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            var processId = session.GetProcessID;
            if (processId == 0) continue;

            string processName = "Unknown";
            try
            {
                var proc = Process.GetProcessById((int)processId);
                processName = proc.ProcessName;
            }
            catch { }

            var volume = session.SimpleAudioVolume.Volume;
            var isMuted = session.SimpleAudioVolume.Mute;

            Console.WriteLine($"[{i}] {processName,-20} | Volume: {(int)(volume * 100)}% | Muted: {isMuted}");
        }
    }

    static void MuteOrUnmute()
    {
        var sessions = GetCurrentSessions();
        Console.Write($"\nEnter session number to toggle mute");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < sessions.Count)
        {
            try
            {
                var mute = sessions[index].SimpleAudioVolume.Mute;
                sessions[index].SimpleAudioVolume.Mute = !mute;
                Console.WriteLine(mute ? "Unmuted." : "Muted.");
            }
            catch
            {
                Console.WriteLine("Failed to change mute state.");
            }
        }
        else
        {
            Console.WriteLine("Invalid session index.");
        }
    }

    static void SetVolume()
    {
        var sessions = GetCurrentSessions();
        Console.Write("\nEnter session number to set volume: ");
        if (!int.TryParse(Console.ReadLine(), out int index) || index < 0 || index >= sessions.Count)
        {
            Console.WriteLine("Invalid session index.");
            return;
        }

        Console.Write("Enter new volume (0–100): ");
        if (!float.TryParse(Console.ReadLine(), out float percent) || percent < 0 || percent > 100)
        {
            Console.WriteLine("Invalid volume.");
            return;
        }

        try
        {
            sessions[index].SimpleAudioVolume.Volume = percent / 100f;
            Console.WriteLine($"Volume set to {percent}%.");
        }
        catch
        {
            Console.WriteLine("Failed to set volume.");
        }
    }

    static SessionCollection GetCurrentSessions()
    {
        var deviceEnum = new MMDeviceEnumerator();
        var device = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return device.AudioSessionManager.Sessions;
    }
}

