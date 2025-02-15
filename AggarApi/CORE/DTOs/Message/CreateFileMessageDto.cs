namespace CORE.DTOs.Message
{
    public class CreateFileMessageDto : CreateMessageDto
    {
        public string FilePath { get; set; }
        public string? Checksum { get; set; }
        public byte[]? Bytes { get; set; }
    }
}
