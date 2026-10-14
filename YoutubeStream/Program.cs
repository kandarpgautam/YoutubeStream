using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Diagnostics;

string url = args.FirstOrDefault() ?? string.Empty;

try
{
    if (string.IsNullOrWhiteSpace(url))
    {
        Console.WriteLine("Please provide a YouTube video URL as a command-line argument. For example: youtubestream <video_url>");
        Console.ReadKey();
        return;
    }

    var youtube = new YoutubeClient();

    // Get video info
    var video = await youtube.Videos.GetAsync(url);
    Console.WriteLine($"Title: {video.Title}");

    // Get stream manifest
    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);

    // Select best audio-only stream
    var audioStreamInfo = streamManifest.GetAudioOnlyStreams()
                                        .GetWithHighestBitrate();

    if (audioStreamInfo == null)
    {
        Console.WriteLine("No audio stream found.");
        return;
    }

    Console.WriteLine("Streaming audio...");

    using var ms = new MemoryStream();

    using var stream = await youtube.Videos.Streams.GetAsync(audioStreamInfo);
    await stream.CopyToAsync(ms);
    ms.Position = 0;

    // Save to a temporary MP3 file and open with the default player
    var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".mp3");
    ms.Position = 0;
    using (var fs = File.Create(tempPath))
        await ms.CopyToAsync(fs);

    Console.WriteLine($"Saved audio to: {tempPath}");
    Console.WriteLine("Opening default player...");
    var psi = new ProcessStartInfo(tempPath) { UseShellExecute = true };
    Process.Start(psi);
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
