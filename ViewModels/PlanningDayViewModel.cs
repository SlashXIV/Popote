using System.Collections.ObjectModel;
using Popote.Models;

namespace Popote.ViewModels;

// Un jour de la semaine dans le planning, avec ses repas prévus.
public class PlanningDayViewModel
{
    public PlanningDayViewModel(DateTime date, string label)
    {
        Date = date;
        Label = label;
    }

    public DateTime Date { get; }
    public string Label { get; }
    public ObservableCollection<PlannedMeal> Meals { get; } = new();
}
