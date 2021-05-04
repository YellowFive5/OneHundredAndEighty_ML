namespace BinaryLearning
{
    public class InputData
    {
        public byte[] Img { get; set; }
        public uint LabelKey { get; set; }
        public string ImgPath { get; set; } //path of the image
        public string Label { get; set; }
    }
}