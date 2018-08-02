<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:Label ID="debug" runat="server" />
    <div>
      <p><b>Sources: </b><asp:FileUpload ID="sourcesFU" runat="server" Multiple="Multiple"/></p>
      <p><b>Task: </b>
          <asp:DropDownList ID="taskDDL" runat="server">
              <asp:ListItem Text="Fast Harris + RANSAC + Blend" Value="fastHarrisRansacBlend"></asp:ListItem>
              <asp:ListItem Text="Harris + RANSAC + Blend" Value="harrisRansacBlend"></asp:ListItem>
              <asp:ListItem Text="SURF + RANSAC + Blend" Value="surfRansacBlend"></asp:ListItem>
              <asp:ListItem Text="FREAK + RANSAC + Blend" Value="freakRansacBlend"></asp:ListItem>
              <asp:ListItem Text="Fast Harris + RANSAC + Blend + Straight" Value="fastHarrisRansacBlendStraight"></asp:ListItem>
              <asp:ListItem Text="Harris + RANSAC + Blend + Straight" Value="harrisRansacBlendStraight"></asp:ListItem>
              <asp:ListItem Text="SURF + RANSAC + Blend + Straight" Value="surfRansacBlendStraight"></asp:ListItem>
              <asp:ListItem Text="Fast Harris Feature Correlation" Value="fastHarrisFeaturesCorrelation"></asp:ListItem>
              <asp:ListItem Text="Harris Feature Correlation" Value="harrisFeaturesCorrelation"></asp:ListItem>
              <asp:ListItem Text="SURF Feature Correlation" Value="surfFeaturesCorrelation"></asp:ListItem>
              <asp:ListItem Text="FREAK Feature Correlation" Value="freakFeaturesCorrelation"></asp:ListItem>
              <asp:ListItem Text="Harris Feature Detection" Value="harrisFeatures"></asp:ListItem>
              <asp:ListItem Text="SURF Feature Detection" Value="surfFeatures"></asp:ListItem>
              <asp:ListItem Text="FREAK Feature Detection" Value="freakFeatures"></asp:ListItem>
          </asp:DropDownList>

      </p>
      <asp:Button ID="runBtn" text="Run!" OnClick="run" runat="server" />
      <asp:Button ID="clearBtn" text="Clear!" OnClick="clear" runat="server" />
      <hr />
      <asp:ScriptManager ID="ScriptManager1" runat="server" />
      <asp:UpdatePanel runat="server" ID="imageUP">
       <ContentTemplate></ContentTemplate>
      </asp:UpdatePanel>
    </div>
    </form>
</body>
</html>

