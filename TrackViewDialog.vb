Imports System.Drawing
Imports System.Windows.Forms

Public Class TrackViewDialog
    Inherits Form

    Private hierarchyTreeView As TreeView
    Private controllerListView As ListView
    Private toolbar As ToolStrip

    Public Sub New()
        Me.Text = "Track View - Curve Editor"
        Me.Size = New Size(900, 700)
        Me.StartPosition = FormStartPosition.CenterScreen
        InitializeComponents()
    End Sub

    Private Sub InitializeComponents()
        ' Create toolbar
        toolbar = New ToolStrip()
        toolbar.Items.Add("Filters")
        toolbar.Items.Add("Assign Controller")
        toolbar.Items.Add("Copy")
        toolbar.Items.Add("Paste")

        ' Create split container
        Dim splitContainer As New SplitContainer With {
            .Dock = DockStyle.Fill,
            .Orientation = Orientation.Vertical,
            .SplitterDistance = 300
        }

        ' Left panel - Scene hierarchy
        hierarchyTreeView = New TreeView With {
            .Dock = DockStyle.Fill,
            .CheckBoxes = True
        }

        ' Add sample nodes
        Dim rootNode = hierarchyTreeView.Nodes.Add("Scene Root")
        rootNode.Nodes.Add("Objects")
        rootNode.Nodes.Add("Materials")
        rootNode.Nodes.Add("Global Tracks")
        rootNode.ExpandAll()

        ' Right panel - Controller details
        controllerListView = New ListView With {
            .Dock = DockStyle.Fill,
            .View = View.Details,
            .FullRowSelect = True,
            .GridLines = True
        }

        controllerListView.Columns.Add("Controller", 200)
        controllerListView.Columns.Add("Value", 100)
        controllerListView.Columns.Add("In", 80)
        controllerListView.Columns.Add("Out", 80)

        splitContainer.Panel1.Controls.Add(hierarchyTreeView)
        splitContainer.Panel2.Controls.Add(controllerListView)

        Me.Controls.Add(splitContainer)
        Me.Controls.Add(toolbar)
    End Sub
End Class