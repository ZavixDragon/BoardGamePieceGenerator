namespace Generator
{
    class Program
    {
        static void Main(string[] args) => args.ForEach(x => new Generator().Run(x)); 
    }
}
