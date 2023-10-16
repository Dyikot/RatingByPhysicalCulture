namespace TestDataForRatingByPhysicalCultural
{
	internal class Program
	{
		static void Main(string[] args)
		{
			WriteToFile(filePath: "dataForTest.txt", columnAmount: 5, rowAmount: 100);
		}

		static void WriteToFile(string filePath, int columnAmount, int rowAmount)
		{
			string name, group, value, time;
			var random = new Random();
			using (var streamWriter = new StreamWriter(filePath, append: false))
			{
				for(int j = 0; j < rowAmount; j++)
				{
					name = $"Name{random.Next(0, 1000)} Second{random.Next(0, 1000)} Thrird";
					group = $"Group{random.Next(0, 1000)}";
					streamWriter.Write($"{name}\t{group}");

					for (int i = 0; i < columnAmount; i++)
					{
						value = random.Next(10, 500).ToString();
						time = $"{random.Next(0, 59)}:{random.Next(0, 59)}";
						streamWriter.Write($"\t{value}\t{time}");
					}

					if(j < rowAmount - 1)
					{
						streamWriter.WriteLine();
					}
				}
			}
		}
	}
}