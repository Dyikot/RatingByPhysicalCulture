using RatingByPhysicalCulture.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace RatingByPhysicalCulture.ViewModel
{
	class ProjectListModel
	{
		private readonly ListBox _projectList;
		private ObservableCollection<Project> _projects;
		private ObservableCollection<Project> Projects
		{
			get => _projects;
			set
			{
				_projects = value;
				_projectList.ItemsSource = value;
			}
		}

		public ProjectListModel(ListBox projectList)
		{
			_projectList = projectList;
			InitializeProjectList();
		}

		public void AddProject(Project project) => Projects.Add(project);
		public void RemoveProject(Project project) => Projects.Remove(project);
		public void RemoveProject(Predicate<Project> match)
		{
			var projects = Projects.ToList();
			projects.RemoveAll(match);
			Projects = new ObservableCollection<Project>(projects);
		}
		public void UpdateTime(Project project)
		{
			project.TimeOpened = DateTime.Now.ToString(Project.TimeFortmat);
		}
		public void SerializeProjects()
		{
			using (var fileStream = new FileStream(
				ProjectInfo.GetInstance.ProjectsList,
				FileMode.Create
				))
			{

				new XmlSerializer(typeof(List<Project>)).Serialize(
					fileStream,
					Projects.ToList()
					);
			}
		}

		private void InitializeProjectList()
		{
			if (File.Exists(ProjectInfo.GetInstance.ProjectsList))
			{
				Projects = new ObservableCollection<Project>(DeserializeProjectList());
			}
			else
			{
				Projects = new ObservableCollection<Project>();
			}
		}
		private List<Project> DeserializeProjectList()
		{
			var projects = new List<Project>();
			using (var fileStream = new FileStream(
				ProjectInfo.GetInstance.ProjectsList,
				FileMode.Open
				))
			{
				projects = new XmlSerializer(typeof(List<Project>))
					.Deserialize(fileStream) as List<Project>;
			}

			var sortedProjects = projects
				.OrderByDescending(project => project.TimeOpened)
				.ToList();

			return sortedProjects!;
		}	
	}
}
