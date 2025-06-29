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
            var sessions = GetCurrentSessions();
            ShowSessions();

            Console.WriteLine("");
            Console.WriteLine("Session controls: Press a number to select a session");
            Console.WriteLine("Spotify controls: [p] Play/Pause | [n] Next | [b] Previous");
            Console.WriteLine("General:          [r] Refresh | [q] Quit\n");

            var keyInfo = Console.ReadKey(intercept: true);
            char input = keyInfo.KeyChar;

            if (char.IsDigit(input))
            {
                int sessionIndex = int.Parse(input.ToString());
                if (sessionIndex >= 0 && sessionIndex < sessions.Count)
                {
                    ControlSessionLoop(sessions[sessionIndex]);
                    continue;
                }
            }

            switch (keyInfo.Key)
            {
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

    static void ControlSessionLoop(AudioSessionControl session)
    {
        while (true)
        {
            Console.Clear();
            var procName = session.DisplayName;
            var vol = session.SimpleAudioVolume.Volume;
            var muted = session.SimpleAudioVolume.Mute;
            Console.WriteLine(
                $"Selected Session: {procName} | Volume: {(int)(vol * 100)}% | Muted: {muted}");
            Console.WriteLine("\nControls:");
            Console.WriteLine("[m] Mute/Unmute");
            Console.WriteLine("[v] Set Volume");
            Console.WriteLine("[r] Return to session list\n");

            var key = Console.ReadKey(intercept: true).Key;

            switch (key)
            {
                case ConsoleKey.M:
                    session.SimpleAudioVolume.Mute = !session.SimpleAudioVolume.Mute;
                    break;
                case ConsoleKey.V:
                    Console.Write("Enter new volume (0-100): ");
                    if (float.TryParse(Console.ReadLine(), out float percent) && percent >= 0 && percent <= 100)
                        session.SimpleAudioVolume.Volume = percent / 100f;
                    else
                    {
                        Console.WriteLine("Invalid volume. Press any key...");
                        Console.ReadKey(intercept: true);
                    }
                    break;
                case ConsoleKey.R:
                    return;
                default:
                    Console.WriteLine("Invalid key. Press any key...");
                    Console.ReadKey(intercept: true);
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

    static SessionCollection GetCurrentSessions()
    {
        var deviceEnum = new MMDeviceEnumerator();
        var device = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return device.AudioSessionManager.Sessions;
    }
}

