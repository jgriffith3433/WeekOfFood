using ContainerNinja.Contracts.Services;
using Microsoft.AspNetCore.Hosting;
using NAudio.Wave;

namespace ContainerNinja.Core.Services
{
    public class NAudioService : INAudioService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NAudioService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public byte[] StripNoise(byte[] data)
        {
            using (var memReader = new MemoryStream(data))
            {
                using (var waveFileReader = new WaveFileReader(memReader))
                {
                    var strippedData = StripAudioNoise(waveFileReader, new TimeSpan(0, 0, 1), new TimeSpan(0, 0, 2));
                    using (var memWriter = new MemoryStream(strippedData))
                    {
                        using (var writer = new WaveFileWriter(memWriter, waveFileReader.WaveFormat))
                        {
                            writer.Write(memWriter.ToArray(), 0, (int)memWriter.Length);
                        }
                        memWriter.Position = 0;
                        return memWriter.GetBuffer();
                    }
                }
            }
        }

        private byte[] StripAudioNoise(WaveFileReader waveFileReader, TimeSpan cutFromStart, TimeSpan cutFromEnd)
        {
            return null;
            //https://github.com/naudio/NAudio/blob/master/Docs/WaveProviders.md
            //int bytesPerMillisecond = waveFileReader.WaveFormat.AverageBytesPerSecond / 1000;

            //int startPos = (int)cutFromStart.TotalMilliseconds * bytesPerMillisecond;
            //startPos = startPos - startPos % waveFileReader.WaveFormat.BlockAlign;

            //int endBytes = (int)cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
            //endBytes = endBytes - endBytes % waveFileReader.WaveFormat.BlockAlign;
            //int endPos = (int)waveFileReader.Length - endBytes;

            //waveFileReader.Position = startPos;
            //byte[] buffer = new byte[1024];
            //while (waveFileReader.Position < endPos)
            //{
            //    int bytesRequired = (int)(endPos - waveFileReader.Position);
            //    if (bytesRequired > 0)
            //    {
            //        int bytesToRead = Math.Min(bytesRequired, buffer.Length);
            //        int bytesRead = waveFileReader.Read(buffer, 0, bytesToRead);
            //        if (bytesRead > 0)
            //        {
            //            waveFileWriter.WriteData(buffer, 0, bytesRead);
            //        }
            //    }
            //}
        }
    }
}
