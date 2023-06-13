
namespace ContainerNinja.Contracts.Services
{
    public interface INAudioService
    {
        byte[] StripNoise(byte[] data);
    }
}
