using System;
using System.Collections.Generic;
using System.Text;

namespace Rimshot.Tools {
  public static class BCFExport {
    public const string Plugin = "Rimshot.BCFPlugin";
    public const string Command = "Rimshot_Button_ExportBCF";
  }
  public class IssueList {
    public const string Plugin = "Rimshot.WorkshopIssueListPlugin";
    public const string Command = "Rimshot_Button_ShowIssueList";
  }
  public class IssueRegister {
    public const string Plugin = "Rimshot.IssueRegisterPlugin";
    public const string Command = "Rimshot_Button_ShowIssueRegister";
  }
  public class AnalysisRegister {
    public const string Plugin = "Rimshot.AnalysisRegisterPlugin";
    public const string Command = "Rimshot_Button_ShowAnalysisRegister";
  }
  public class TestRunner {
    public const string Plugin = "Rimshot.TestRunnerPlugin";
    public const string Command = "Rimshot_Button_SelectiveUpdates";
  }

  public class ClashElements {
    public const string Plugin = "Rimshot.ClashElementsPlugin";
    public const string Command = "Rimshot_Button_ClashElements";
  }

  public class ShowOnly {
    public const string Command = "Rimshot_Button_ShowOnlySelected";
  }

  public class ShowAlso {
    public const string Command = "Rimshot_Button_ShowAlsoSelected";
  }
}