Imports SharpDX
Imports D3D11 = SharpDX.Direct3D11  ' Use alias to avoid conflicts
Imports SharpDX.Direct3D
Imports SharpDX.DXGI
Imports SharpDX.Mathematics.Interop
Imports SharpDX.D3DCompiler
Imports System.Runtime.InteropServices
Imports System.Drawing  ' For Point disambiguation

Public Class ViewportPanel
    Inherits Panel

    ' DirectX objects - use the aliased namespace
    Private device As D3D11.Device
    Private deviceContext As D3D11.DeviceContext
    Private swapChain As SwapChain
    Private renderTargetView As D3D11.RenderTargetView
    Private depthStencilView As D3D11.DepthStencilView
    Private depthStencilState As D3D11.DepthStencilState

    ' Shaders
    Private vertexShader As D3D11.VertexShader
    Private pixelShader As D3D11.PixelShader
    Private inputLayout As D3D11.InputLayout
    Private constantBuffer As D3D11.Buffer

    ' Viewport properties
    Private viewType As String
    Private camera As ViewportCamera
    Private objects As List(Of Object3D)
    Private showGrid As Boolean = True
    Private gridSettings As GridSettings
    Private isActive As Boolean = False

    ' Grid rendering
    Private gridVertexBuffer As D3D11.Buffer
    Private gridIndexBuffer As D3D11.Buffer
    Private gridVertexCount As Integer

    ' Mouse interaction
    Private lastMousePos As System.Drawing.Point  ' Use full name
    Private isMouseDown As Boolean

    Public Sub New(viewType As String)
        Me.viewType = viewType
        Me.camera = New ViewportCamera(viewType)
        Me.objects = New List(Of Object3D)
        Me.gridSettings = New GridSettings()

        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint, True)
        SetStyle(ControlStyles.Opaque, True)
        SetStyle(ControlStyles.ResizeRedraw, True)

        ' Enable double buffering
        SetStyle(ControlStyles.DoubleBuffer, True)
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        InitializeDirectX()
        InitializeShaders()
        CreateGridGeometry()
    End Sub

    Private Sub InitializeDirectX()
        Try
            ' Create swap chain description
            Dim swapChainDesc As New SwapChainDescription() With {
                .BufferCount = 1,
                .ModeDescription = New ModeDescription(
                    ClientSize.Width,
                    ClientSize.Height,
                    New Rational(60, 1),
                    Format.R8G8B8A8_UNorm
                ),
                .IsWindowed = True,
                .OutputHandle = Handle,
                .SampleDescription = New SampleDescription(1, 0),
                .SwapEffect = SwapEffect.Discard,
                .Usage = Usage.RenderTargetOutput
            }

            ' Create device and swap chain - Fix FeatureLevel reference
            Dim featureLevels() As FeatureLevel = {FeatureLevel.Level_11_0}

            D3D11.Device.CreateWithSwapChain(
                DriverType.Hardware,
                D3D11.DeviceCreationFlags.None,
                featureLevels,
                swapChainDesc,
                device,
                swapChain
            )

            deviceContext = device.ImmediateContext

            ' Ignore all windows events
            Using factory = swapChain.GetParent(Of Factory)()
                factory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAll)
            End Using

            CreateRenderTargets()
            CreateDepthStencilState()
        Catch ex As Exception
            MessageBox.Show($"Failed to initialize DirectX: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub CreateRenderTargets()
        ' Create render target view
        Using backBuffer = swapChain.GetBackBuffer(Of D3D11.Texture2D)(0)
            renderTargetView = New D3D11.RenderTargetView(device, backBuffer)
        End Using

        ' Create depth stencil
        Dim depthStencilDesc As New D3D11.Texture2DDescription() With {
            .Format = Format.D24_UNorm_S8_UInt,
            .ArraySize = 1,
            .MipLevels = 1,
            .Width = ClientSize.Width,
            .Height = ClientSize.Height,
            .SampleDescription = New SampleDescription(1, 0),
            .Usage = D3D11.ResourceUsage.Default,
            .BindFlags = D3D11.BindFlags.DepthStencil,
            .CpuAccessFlags = D3D11.CpuAccessFlags.None,
            .OptionFlags = D3D11.ResourceOptionFlags.None
        }

        Using depthStencilBuffer = New D3D11.Texture2D(device, depthStencilDesc)
            depthStencilView = New D3D11.DepthStencilView(device, depthStencilBuffer)
        End Using

        ' Set viewport
        Dim viewport As New Viewport(0, 0, ClientSize.Width, ClientSize.Height, 0.0F, 1.0F)
        deviceContext.Rasterizer.SetViewport(viewport)

        ' Set render targets
        deviceContext.OutputMerger.SetTargets(depthStencilView, renderTargetView)
    End Sub

    Private Sub CreateDepthStencilState()
        Dim depthStencilStateDesc As New D3D11.DepthStencilStateDescription() With {
            .IsDepthEnabled = True,
            .DepthWriteMask = D3D11.DepthWriteMask.All,
            .DepthComparison = D3D11.Comparison.Less,
            .IsStencilEnabled = False
        }

        depthStencilState = New D3D11.DepthStencilState(device, depthStencilStateDesc)
        deviceContext.OutputMerger.SetDepthStencilState(depthStencilState)
    End Sub

    Private Sub InitializeShaders()
        ' Vertex shader source
        Dim vertexShaderCode = "
            cbuffer ConstantBuffer : register(b0)
            {
                matrix World;
                matrix View;
                matrix Projection;
                float4 Color;
            }
            
            struct VS_INPUT
            {
                float3 Pos : POSITION;
                float3 Normal : NORMAL;
            };
            
            struct PS_INPUT
            {
                float4 Pos : SV_POSITION;
                float3 Normal : NORMAL;
                float3 WorldPos : TEXCOORD0;
            };
            
            PS_INPUT VS(VS_INPUT input)
            {
                PS_INPUT output = (PS_INPUT)0;
                
                float4 worldPos = mul(float4(input.Pos, 1.0f), World);
                float4 viewPos = mul(worldPos, View);
                output.Pos = mul(viewPos, Projection);
                output.Normal = mul(input.Normal, (float3x3)World);
                output.WorldPos = worldPos.xyz;
                
                return output;
            }"

        ' Pixel shader source
        Dim pixelShaderCode = "
            cbuffer ConstantBuffer : register(b0)
            {
                matrix World;
                matrix View;
                matrix Projection;
                float4 Color;
            }
            
            struct PS_INPUT
            {
                float4 Pos : SV_POSITION;
                float3 Normal : NORMAL;
                float3 WorldPos : TEXCOORD0;
            };
            
            float4 PS(PS_INPUT input) : SV_Target
            {
                float3 lightDir = normalize(float3(1, 1, -1));
                float NdotL = max(dot(normalize(input.Normal), lightDir), 0.2);
                
                return float4(Color.rgb * NdotL, Color.a);
            }"

        ' Compile shaders
        Using vertexShaderBytecode = ShaderBytecode.Compile(vertexShaderCode, "VS", "vs_5_0")
            vertexShader = New D3D11.VertexShader(device, vertexShaderBytecode)

            ' Create input layout
            Dim inputElements = {
                New D3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                New D3D11.InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0)
            }

            inputLayout = New D3D11.InputLayout(device, vertexShaderBytecode, inputElements)
        End Using

        Using pixelShaderBytecode = ShaderBytecode.Compile(pixelShaderCode, "PS", "ps_5_0")
            pixelShader = New D3D11.PixelShader(device, pixelShaderBytecode)
        End Using

        ' Create constant buffer
        Dim constantBufferDesc As New D3D11.BufferDescription() With {
            .Usage = D3D11.ResourceUsage.Default,
            .SizeInBytes = Marshal.SizeOf(GetType(ConstantBufferData)),
            .BindFlags = D3D11.BindFlags.ConstantBuffer,
            .CpuAccessFlags = D3D11.CpuAccessFlags.None
        }

        constantBuffer = New D3D11.Buffer(device, constantBufferDesc)

        ' Set shaders
        deviceContext.VertexShader.Set(vertexShader)
        deviceContext.PixelShader.Set(pixelShader)
        deviceContext.VertexShader.SetConstantBuffer(0, constantBuffer)
        deviceContext.PixelShader.SetConstantBuffer(0, constantBuffer)
        deviceContext.InputAssembler.InputLayout = inputLayout
    End Sub

    Private Sub CreateGridGeometry()
        Dim vertices As New List(Of VertexData)
        Dim indices As New List(Of Integer)

        Dim gridSize = 1000.0F
        Dim spacing = gridSettings.GridSpacing
        Dim majorInterval = gridSettings.MajorLineInterval

        Dim index = 0

        ' Create grid lines
        Dim lineCount = CInt(gridSize / spacing)

        For i = -lineCount To lineCount
            Dim pos = i * spacing
            Dim isMajor = (i Mod majorInterval = 0)

            ' Vertical line
            vertices.Add(New VertexData(New Vector3(pos, 0, -gridSize), Vector3.UnitY))
            vertices.Add(New VertexData(New Vector3(pos, 0, gridSize), Vector3.UnitY))
            indices.Add(index)
            indices.Add(index + 1)
            index += 2

            ' Horizontal line
            vertices.Add(New VertexData(New Vector3(-gridSize, 0, pos), Vector3.UnitY))
            vertices.Add(New VertexData(New Vector3(gridSize, 0, pos), Vector3.UnitY))
            indices.Add(index)
            indices.Add(index + 1)
            index += 2
        Next

        gridVertexCount = indices.Count

        ' Create vertex buffer
        Dim vertexBufferDesc As New D3D11.BufferDescription() With {
            .Usage = D3D11.ResourceUsage.Default,
            .SizeInBytes = Marshal.SizeOf(GetType(VertexData)) * vertices.Count,
            .BindFlags = D3D11.BindFlags.VertexBuffer,
            .CpuAccessFlags = D3D11.CpuAccessFlags.None
        }

        gridVertexBuffer = D3D11.Buffer.Create(device, vertices.ToArray(), vertexBufferDesc)

        ' Create index buffer
        Dim indexBufferDesc As New D3D11.BufferDescription() With {
            .Usage = D3D11.ResourceUsage.Default,
            .SizeInBytes = Marshal.SizeOf(GetType(Integer)) * indices.Count,
            .BindFlags = D3D11.BindFlags.IndexBuffer,
            .CpuAccessFlags = D3D11.CpuAccessFlags.None
        }

        gridIndexBuffer = D3D11.Buffer.Create(device, indices.ToArray(), indexBufferDesc)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        If device Is Nothing Then Return

        RenderScene()
    End Sub

    Private Sub RenderScene()
        ' Clear
        deviceContext.ClearRenderTargetView(renderTargetView, New Color4(0.1F, 0.1F, 0.1F, 1.0F))
        deviceContext.ClearDepthStencilView(depthStencilView, D3D11.DepthStencilClearFlags.Depth, 1.0F, 0)

        ' Set primitive topology
        deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList

        ' Setup camera matrices
        Dim viewMatrix = camera.GetViewMatrix()
        Dim projectionMatrix = camera.GetProjectionMatrix(CSng(ClientSize.Width) / ClientSize.Height)

        ' Render grid
        If showGrid Then
            RenderGrid(viewMatrix, projectionMatrix)
        End If

        ' Render objects
        For Each obj In objects
            RenderObject(obj, viewMatrix, projectionMatrix)
        Next

        ' Draw viewport label
        DrawViewportLabel()

        ' Present
        swapChain.Present(0, PresentFlags.None)
    End Sub

    Private Sub RenderGrid(viewMatrix As Matrix, projectionMatrix As Matrix)
        ' Set vertex buffer
        deviceContext.InputAssembler.SetVertexBuffers(0, New D3D11.VertexBufferBinding(
            gridVertexBuffer, Marshal.SizeOf(GetType(VertexData)), 0))
        deviceContext.InputAssembler.SetIndexBuffer(gridIndexBuffer, Format.R32_UInt, 0)

        ' Update constant buffer
        Dim cbData As New ConstantBufferData() With {
            .World = Matrix.Identity,
            .View = viewMatrix,
            .Projection = projectionMatrix,
            .Color = New Vector4(0.3F, 0.3F, 0.3F, 1.0F)
        }

        deviceContext.UpdateSubresource(cbData, constantBuffer)

        ' Draw
        deviceContext.DrawIndexed(gridVertexCount, 0, 0)
    End Sub

    Private Sub RenderObject(obj As Object3D, viewMatrix As Matrix, projectionMatrix As Matrix)
        ' Get object mesh
        Dim mesh = MeshGenerator.GetMesh(obj.Type, device)
        If mesh Is Nothing Then Return

        ' Set vertex buffer
        deviceContext.InputAssembler.SetVertexBuffers(0, New D3D11.VertexBufferBinding(
            mesh.VertexBuffer, Marshal.SizeOf(GetType(VertexData)), 0))
        deviceContext.InputAssembler.SetIndexBuffer(mesh.IndexBuffer, Format.R32_UInt, 0)

        ' Create world matrix
        Dim worldMatrix = Matrix.Scaling(obj.Scale.X, obj.Scale.Y, obj.Scale.Z) *
                         Matrix.RotationYawPitchRoll(obj.Rotation.Y, obj.Rotation.X, obj.Rotation.Z) *
                         Matrix.Translation(obj.Position.X, obj.Position.Y, obj.Position.Z)

        ' Update constant buffer
        Dim cbData As New ConstantBufferData() With {
            .World = worldMatrix,
            .View = viewMatrix,
            .Projection = projectionMatrix,
            .Color = If(obj.IsSelected, New Vector4(1, 1, 0, 1), New Vector4(0.8F, 0.8F, 0.8F, 1))
        }

        deviceContext.UpdateSubresource(cbData, constantBuffer)

        ' Set to triangle list for solid objects
        deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList

        ' Draw
        deviceContext.DrawIndexed(mesh.IndexCount, 0, 0)

        ' Reset to line list
        deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList
    End Sub

    Private Sub DrawViewportLabel()
        ' This would use Direct2D or GDI+ overlay
        ' For now, we'll skip the label rendering
    End Sub

    ' Mouse interaction
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        isMouseDown = True
        lastMousePos = e.Location
        Focus()
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)

        If isMouseDown AndAlso e.Button = MouseButtons.Middle Then
            Dim deltaX = e.X - lastMousePos.X
            Dim deltaY = e.Y - lastMousePos.Y

            If viewType = "Perspective" Then
                camera.Rotate(deltaX * 0.01F, deltaY * 0.01F)
                Invalidate()
            End If

            lastMousePos = e.Location
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        isMouseDown = False
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        camera.Zoom(e.Delta / 120.0F)
        Invalidate()
    End Sub

    ' Public methods
    Public Sub UpdateObjects(newObjects As List(Of Object3D))
        objects = newObjects
        Invalidate()
    End Sub

    Public Sub SetActive(active As Boolean)
        isActive = active
        Invalidate()
    End Sub

    Public Sub SetShowGrid(show As Boolean)
        showGrid = show
        Invalidate()
    End Sub

    Public Sub UpdateGridSettings(settings As GridSettings)
        gridSettings = settings
        CreateGridGeometry()
        Invalidate()
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)

        If device IsNot Nothing AndAlso ClientSize.Width > 0 AndAlso ClientSize.Height > 0 Then
            ' Dispose old views
            renderTargetView?.Dispose()
            depthStencilView?.Dispose()

            ' Resize swap chain
            swapChain.ResizeBuffers(1, ClientSize.Width, ClientSize.Height, Format.Unknown, SwapChainFlags.None)

            ' Recreate render targets
            CreateRenderTargets()

            Invalidate()
        End If
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            ' Dispose DirectX objects
            constantBuffer?.Dispose()
            inputLayout?.Dispose()
            pixelShader?.Dispose()
            vertexShader?.Dispose()
            gridIndexBuffer?.Dispose()
            gridVertexBuffer?.Dispose()
            depthStencilState?.Dispose()
            depthStencilView?.Dispose()
            renderTargetView?.Dispose()
            swapChain?.Dispose()
            deviceContext?.Dispose()
            device?.Dispose()

            ' Dispose meshes
            MeshGenerator.DisposeMeshes()
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class

' ViewportCamera class is OK to keep here
Public Class ViewportCamera
    Public Property Position As Vector3
    Public Property Target As Vector3
    Public Property Up As Vector3
    Public Property FieldOfView As Single
    Public Property NearPlane As Single
    Public Property FarPlane As Single

    Private rotationX As Single
    Private rotationY As Single
    Private distance As Single

    Public Sub New(viewType As String)
        Up = Vector3.UnitY
        FieldOfView = MathUtil.PiOverFour
        NearPlane = 0.1F
        FarPlane = 10000.0F

        Select Case viewType
            Case "Top"
                Position = New Vector3(0, 1000, 0)
                Target = Vector3.Zero
                distance = 1000
            Case "Front"
                Position = New Vector3(0, 0, 1000)
                Target = Vector3.Zero
                distance = 1000
            Case "Left"
                Position = New Vector3(-1000, 0, 0)
                Target = Vector3.Zero
                distance = 1000
            Case "Perspective"
                distance = 500
                rotationX = -0.5F
                rotationY = 0.785F ' 45 degrees
                UpdatePosition()
        End Select
    End Sub

    Private Sub UpdatePosition()
        Dim x = distance * Math.Cos(rotationX) * Math.Sin(rotationY)
        Dim y = distance * Math.Sin(rotationX)
        Dim z = distance * Math.Cos(rotationX) * Math.Cos(rotationY)

        Position = New Vector3(CSng(x), CSng(y), CSng(z))
    End Sub

    Public Sub Rotate(deltaX As Single, deltaY As Single)
        rotationY += deltaX
        rotationX = Math.Max(-1.5F, Math.Min(1.5F, rotationX + deltaY))
        UpdatePosition()
    End Sub

    Public Sub Zoom(delta As Single)
        distance = Math.Max(10, Math.Min(5000, distance - delta * 50))
        UpdatePosition()
    End Sub

    Public Function GetViewMatrix() As Matrix
        Return Matrix.LookAtLH(Position, Target, Up)
    End Function

    Public Function GetProjectionMatrix(aspectRatio As Single) As Matrix
        Return Matrix.PerspectiveFovLH(FieldOfView, aspectRatio, NearPlane, FarPlane)
    End Function
End Class