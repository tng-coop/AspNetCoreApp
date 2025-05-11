using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public interface ITreeMenuService
    {
        Task<List<MenuItem>> GetMenuAsync();
    }

    public class TreeMenuService : ITreeMenuService
    {
        public Task<List<MenuItem>> GetMenuAsync()
        {
            var now = DateTime.Now;
            var timeGroup = new MenuItem
            {
                Title = $"Current Time: {now:HH:mm:ss}",
                IconCss = "bi bi-clock",
                Children = new List<MenuItem>
                {
                    new MenuItem { Title = $"Hour: {now.Hour}",   IconCss = "bi bi-hourglass-top"    },
                    new MenuItem { Title = $"Minute: {now.Minute}", IconCss = "bi bi-hourglass-split"  },
                    new MenuItem { Title = $"Second: {now.Second}",IconCss = "bi bi-hourglass-bottom" }
                }
            };
            var rnd = new Random();
            var groups = new List<MenuItem> { timeGroup };
            for (int i = 2; i <= 3; i++)
            {
                var grp = new MenuItem { Title = $"Group {i}", IconCss = "bi bi-folder", Children = new List<MenuItem>() };
                for (int j = 1; j <= 3; j++)
                {
                    grp.Children.Add(new MenuItem
                    {
                        Title = $"Item {i}.{j}: {rnd.Next(1000)}",
                        IconCss = "bi bi-file-earmark",
                        Url = $"/random/{rnd.Next(1000)}"
                    });
                }
                groups.Add(grp);
            }
            return Task.FromResult(groups);
        }
    }
}
