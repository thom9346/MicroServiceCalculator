using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebUI.Pages.Models;

namespace WebUI.Pages
{
    public class IndexModel : PageModel
    {
        public CalculatorModel Calc { get; set; } = new CalculatorModel();

        public void OnGet()
        {
        }

        public void OnPost()
        {
            switch (Calc.Operation)
            {
                case "add":
                    Calc.Result = Calc.FirstNumber + Calc.SecondNumber;
                    break;
                case "subtract":
                    Calc.Result = Calc.FirstNumber - Calc.SecondNumber;
                    break;
                case "multiply":
                    Calc.Result = Calc.FirstNumber * Calc.SecondNumber;
                    break;
                case "divide":
                    if (Calc.SecondNumber == 0)
                    {
                        ModelState.AddModelError("", "Cannot divide by zero!");
                        return;
                    }
                    Calc.Result = Calc.FirstNumber / Calc.SecondNumber;
                    break;
            }
        }
    }
}