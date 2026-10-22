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
    public string Label { get; }                                // « Lundi 19 » (titres, TalkBack)
    public string DayName => Date.ToString("ddd");              // « lun. »
    public string DayNumber => Date.Day.ToString();             // « 19 »
    public bool IsToday => Date.Date == DateTime.Today;
    public ObservableCollection<PlannedMeal> Meals { get; } = new();
}
