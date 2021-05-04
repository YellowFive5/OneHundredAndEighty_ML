namespace BinaryLearning
{
    public class InputData
    {
        public byte[] ImageBytes { get; set; }
        public uint LabelKey { get; set; }
        public string ImagePath { get; set; } //path of the image
        public string Label { get; set; }
    }
}