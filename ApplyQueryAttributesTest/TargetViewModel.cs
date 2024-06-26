using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ApplyQueryAttributesTest
{
	public partial class TargetViewModel : ObservableObject, IQueryAttributable
	{
		public TargetViewModel()
		{
		}



        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var message = $"{nameof(ApplyQueryAttributes)} was called with {query.Count()} parameter(s)";
            Console.WriteLine(message);

            Messages.Add(message);
        }


        [ObservableProperty]
        private ObservableCollection<string> _messages = new();

    }

    
}

