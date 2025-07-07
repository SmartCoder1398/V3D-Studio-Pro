Imports System.Drawing
Imports System.Windows.Forms

Public Class DopeSheetDialog
    Inherits Form

    Private dopeSheetPanel As Panel
    Private timelineRuler As Panel
    Private trackListPanel As Panel

    Public Sub New()
        Me.Text = "Dope Sheet"
        Me.Size = New Size(800, 600)
        Me.StartPosition = FormStartPosition.CenterScreen
        InitializeComponents()
    End Sub

    Private Sub InitializeComponents()
        ' Create main container
        Dim mainContainer As New Panel With {
            .Dock = DockStyle.Fill
        }

        ' Timeline ruler at top
        timelineRuler = New Panel With {
            .Height = 30,
            .Dock = DockStyle.Top,
            .BackColor = System.Drawing.Color.FromArgb(60, 60, 60)
        }

        ' Track list on left
        trackListPanel = New Panel With {
            .Width = 200,
            .Dock = DockStyle.Left,
            .BackColor = System.Drawing.Color.FromArgb(50, 50, 50),
            .BorderStyle = BorderStyle.FixedSingle
        }

        ' Dope sheet grid
        dopeSheetPanel = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = System.Drawing.Color.FromArgb(40, 40, 40)
        }

        mainContainer.Controls.Add(dopeSheetPanel)
        mainContainer.Controls.Add(trackListPanel)
        mainContainer.Controls.Add(timelineRuler)

        Me.Controls.Add(mainContainer)

        ' Add toolbar
        Dim toolbar As New ToolStrip()
        toolbar.Items.Add("Select")
        toolbar.Items.Add("Move")
        toolbar.Items.Add("Scale")
        Me.Controls.Add(toolbar)
    End Sub
End Class