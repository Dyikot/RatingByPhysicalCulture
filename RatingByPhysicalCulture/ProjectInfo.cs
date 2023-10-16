using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RatingByPhysicalCulture
{
    public sealed class ProjectInfo
    {
        public string? _projectPath;
        public string? ProjectPath
        {
            get => _projectPath;
            set
            {
                _projectPath = value;
                IsProjectUpToLoad = true;
            }
        }
        public string? ProjectName
        {
            get => Path.GetFileNameWithoutExtension(ProjectPath);
        }
        public string? ProjectDirectory
        {
            get => Path.GetDirectoryName(ProjectPath);
        }
        public string ProjectsList
        {
            get => $"{Environment.CurrentDirectory}\\ProjectsList.xml";
        }
        public string? ColumnsHeaderInfoPath
        {
            get => $"{ProjectDirectory}\\ColumnsHeaderInfo.txt";
        }
        public bool IsProjectUpToLoad { set; get; }

        public static ProjectInfo GetInstance => Instance.Value;
        private static readonly Lazy<ProjectInfo> Instance = new (() => new ProjectInfo());

        private ProjectInfo() { }
    }
}
