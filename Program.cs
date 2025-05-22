using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;

class Program
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private const int SW_RESTORE = 9;

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
    private const byte VK_MEDIA_PLAY_PAUSE = 0xB3;

    static void Main(string[] args)
    {
        while (true)
        {
            ShowSessions();

            Console.WriteLine("");
            Console.WriteLine("Spotify controls: [p] Play/Pause | [n] Next | [b] Previous");
            Console.WriteLine("Session controls: [m] Mute/Unmute | [s] Set Volume");
            Console.WriteLine("General:          [r] Refresh | [q] Quit ");

            var key = Console.ReadKey(intercept: true).Key;

            switch (key)
            {
                case ConsoleKey.M:
                    MuteOrUnmute();
                    break;
                case ConsoleKey.S:
                    SetVolume();
                    break;
                case ConsoleKey.P:
                    SendSpotifyCommand(AppCommand.PlayPause);
                    break;
                case ConsoleKey.N:
                    SendSpotifyCommand(AppCommand.NextTrack);
                    break;
                case ConsoleKey.B:
                    SendSpotifyCommand(AppCommand.PreviousTrack);
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

    static void SendSpotifyCommand(AppCommand command)
    {
        bool success = AppCommandSender.Send("Spotify", command);
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
        Console.Write($"\nEnter session number to toggle mute: ");
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

