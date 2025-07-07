Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Runtime.Serialization

<Serializable>
Public Class Material
    Implements INotifyPropertyChanged, IDisposable

    Private _name As String
    Private _diffuseColor As Color = Color.Gray
    Private _ambientColor As Color = Color.FromArgb(64, 64, 64)
    Private _specularColor As Color = Color.White
    Private _emissiveColor As Color = Color.Black
    Private _shininess As Single = 32.0F
    Private _opacity As Single = 1.0F
    Private _reflection As Single = 0.0F
    Private _refraction As Single = 1.0F
    Private _bumpStrength As Single = 1.0F

    ' PBR Properties
    Private _albedo As Color = Color.Gray
    Private _metallic As Single = 0.0F
    Private _roughness As Single = 0.5F
    Private _ao As Single = 1.0F
    Private _usePBR As Boolean = False

    ' Texture cache
    <NonSerialized>
    Private _textureCache As Dictionary(Of String, Image)

    ' Track disposed state
    <NonSerialized>
    Private _disposed As Boolean = False

    ' Track if we're deserializing
    <NonSerialized>
    Private _isDeserializing As Boolean = False

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    <Category("General")>
    <Description("The name of the material")>
    Public Property Name As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
            OnPropertyChanged("Name")
        End Set
    End Property

    <Category("Basic Material")>
    <Description("Base color of the material")>
    Public Property DiffuseColor As Color
        Get
            Return _diffuseColor
        End Get
        Set(value As Color)
            _diffuseColor = value
            OnPropertyChanged("DiffuseColor")
        End Set
    End Property

    <Category("Basic Material")>
    <Description("Ambient lighting color")>
    Public Property AmbientColor As Color
        Get
            Return _ambientColor
        End Get
        Set(value As Color)
            _ambientColor = value
            OnPropertyChanged("AmbientColor")
        End Set
    End Property

    <Category("Basic Material")>
    <Description("Specular highlight color")>
    Public Property SpecularColor As Color
        Get
            Return _specularColor
        End Get
        Set(value As Color)
            _specularColor = value
            OnPropertyChanged("SpecularColor")
        End Set
    End Property

    <Category("Basic Material")>
    <Description("Self-illumination color")>
    Public Property EmissiveColor As Color
        Get
            Return _emissiveColor
        End Get
        Set(value As Color)
            _emissiveColor = value
            OnPropertyChanged("EmissiveColor")
        End Set
    End Property

    <Category("Basic Material")>
    <Description("Specular highlight size (0-256)")>
    Public Property Shininess As Single
        Get
            Return _shininess
        End Get
        Set(value As Single)
            _shininess = Math.Max(0, Math.Min(256, value))
            OnPropertyChanged("Shininess")
        End Set
    End Property

    <Category("Basic Material")>
    <Description("Material transparency (0-1)")>
    Public Property Opacity As Single
        Get
            Return _opacity
        End Get
        Set(value As Single)
            _opacity = Math.Max(0, Math.Min(1, value))
            OnPropertyChanged("Opacity")
        End Set
    End Property

    <Category("Extended Parameters")>
    <Description("Reflection amount (0-1)")>
    Public Property Reflection As Single
        Get
            Return _reflection
        End Get
        Set(value As Single)
            _reflection = Math.Max(0, Math.Min(1, value))
            OnPropertyChanged("Reflection")
        End Set
    End Property

    <Category("Extended Parameters")>
    <Description("Index of refraction")>
    Public Property Refraction As Single
        Get
            Return _refraction
        End Get
        Set(value As Single)
            _refraction = Math.Max(1, Math.Min(3, value))
            OnPropertyChanged("Refraction")
        End Set
    End Property

    <Category("Maps")>
    <Description("Bump map intensity")>
    Public Property BumpStrength As Single
        Get
            Return _bumpStrength
        End Get
        Set(value As Single)
            _bumpStrength = Math.Max(0, Math.Min(10, value))
            OnPropertyChanged("BumpStrength")
        End Set
    End Property

    ' PBR Properties
    <Category("PBR Material")>
    <Description("Enable Physically Based Rendering")>
    Public Property UsePBR As Boolean
        Get
            Return _usePBR
        End Get
        Set(value As Boolean)
            _usePBR = value
            OnPropertyChanged("UsePBR")
        End Set
    End Property

    <Category("PBR Material")>
    <Description("Base color for PBR workflow")>
    Public Property Albedo As Color
        Get
            Return _albedo
        End Get
        Set(value As Color)
            _albedo = value
            OnPropertyChanged("Albedo")
        End Set
    End Property

    <Category("PBR Material")>
    <Description("Metallic value (0=dielectric, 1=metal)")>
    Public Property Metallic As Single
        Get
            Return _metallic
        End Get
        Set(value As Single)
            _metallic = Math.Max(0, Math.Min(1, value))
            OnPropertyChanged("Metallic")
        End Set
    End Property

    <Category("PBR Material")>
    <Description("Surface roughness (0=smooth, 1=rough)")>
    Public Property Roughness As Single
        Get
            Return _roughness
        End Get
        Set(value As Single)
            _roughness = Math.Max(0, Math.Min(1, value))
            OnPropertyChanged("Roughness")
        End Set
    End Property

    <Category("PBR Material")>
    <Description("Ambient occlusion (0=occluded, 1=no occlusion)")>
    Public Property AO As Single
        Get
            Return _ao
        End Get
        Set(value As Single)
            _ao = Math.Max(0, Math.Min(1, value))
            OnPropertyChanged("AO")
        End Set
    End Property

    ' Texture properties with automatic unloading on change
    Private _diffuseTexture As String
    <Category("Maps")>
    <Description("Diffuse texture map")>
    <Editor(GetType(System.Windows.Forms.Design.FileNameEditor), GetType(System.Drawing.Design.UITypeEditor))>
    Public Property DiffuseTexture As String
        Get
            Return _diffuseTexture
        End Get
        Set(value As String)
            If _diffuseTexture <> value Then
                UnloadTexture(_diffuseTexture)
                _diffuseTexture = value
                OnPropertyChanged("DiffuseTexture")
            End If
        End Set
    End Property

    Private _normalTexture As String
    <Category("Maps")>
    <Description("Normal/Bump texture map")>
    <Editor(GetType(System.Windows.Forms.Design.FileNameEditor), GetType(System.Drawing.Design.UITypeEditor))>
    Public Property NormalTexture As String
        Get
            Return _normalTexture
        End Get
        Set(value As String)
            If _normalTexture <> value Then
                UnloadTexture(_normalTexture)
                _normalTexture = value
                OnPropertyChanged("NormalTexture")
            End If
        End Set
    End Property

    Private _specularTexture As String
    <Category("Maps")>
    <Description("Specular texture map")>
    <Editor(GetType(System.Windows.Forms.Design.FileNameEditor), GetType(System.Drawing.Design.UITypeEditor))>
    Public Property SpecularTexture As String
        Get
            Return _specularTexture
        End Get
        Set(value As String)
            If _specularTexture <> value Then
                UnloadTexture(_specularTexture)
                _specularTexture = value
                OnPropertyChanged("SpecularTexture")
            End If
        End Set
    End Property

    ' PBR Texture Maps with automatic unloading
    Private _albedoTexture As String
    <Category("PBR Maps")>
    <Description("Albedo/Base color texture map")>
    <Editor(GetType(System.Windows.Forms.Design.FileNameEditor), GetType(System.Drawing.Design.UITypeEditor))>
    Public Property AlbedoTexture As String
        Get
            Return _albedoTexture
        End Get
        Set(value As String)
            If _albedoTexture <> value Then
                UnloadTexture(_albedoTexture)
                _albedoTexture = value
                OnPropertyChanged("AlbedoTexture")
            End If
        End Set
    End Property

    Private _metallicTexture As String
    <Category("PBR Maps")>
    <Description("Metallic texture map")>
    <Editor(GetType(System.Windows.Forms.Design.FileNameEditor), GetType(System.Drawing.Design.UITypeEditor))>
    Public Property MetallicTexture As String
        Get
            Return _metallicTexture
        End Get
        Set(value As String)
            If _metallicTexture <> value Then
                UnloadTexture(_metallicTexture)
                _metallicTexture = value
                OnPropertyChanged("MetallicTexture")
            End If
        End Set
    End Property

    Private _roughnessTexture As String
    <Category("PBR Maps")>
    <Description("Roughness texture map")>
    <Editor(GetType(System.Windows.Forms.Design.FileNameEditor), GetType(System.Drawing.Design.UITypeEditor))>
    Public Property RoughnessTexture As String
        Get
            Return _roughnessTexture
        End Get
        Set(value As String)
            If _roughnessTexture <> value Then
                UnloadTexture(_roughnessTexture)
                _roughnessTexture = value
                OnPropertyChanged("RoughnessTexture")
            End If
        End Set
    End Property

    Private _aoTexture As String
    <Category("PBR Maps")>
    <Description("Ambient occlusion texture map")>
    <Editor(GetType(System.Windows.Forms.Design.FileNameEditor), GetType(System.Drawing.Design.UITypeEditor))>
    Public Property AOTexture As String
        Get
            Return _aoTexture
        End Get
        Set(value As String)
            If _aoTexture <> value Then
                UnloadTexture(_aoTexture)
                _aoTexture = value
                OnPropertyChanged("AOTexture")
            End If
        End Set
    End Property

    Private _heightTexture As String
    <Category("PBR Maps")>
    <Description("Height/Displacement texture map")>
    <Editor(GetType(System.Windows.Forms.Design.FileNameEditor), GetType(System.Drawing.Design.UITypeEditor))>
    Public Property HeightTexture As String
        Get
            Return _heightTexture
        End Get
        Set(value As String)
            If _heightTexture <> value Then
                UnloadTexture(_heightTexture)
                _heightTexture = value
                OnPropertyChanged("HeightTexture")
            End If
        End Set
    End Property

    ' Additional properties
    <Category("Special Effects")>
    <Description("Render both sides of faces")>
    Public Property TwoSided As Boolean = False

    <Category("Special Effects")>
    <Description("Object casts shadows")>
    Public Property CastShadows As Boolean = True

    <Category("Special Effects")>
    <Description("Object receives shadows")>
    Public Property ReceiveShadows As Boolean = True

    <Category("Special Effects")>
    <Description("Wire frame rendering")>
    Public Property Wireframe As Boolean = False

    <Category("Special Effects")>
    <Description("Face normals rendering mode")>
    Public Property FacetedShading As Boolean = False

    Public Sub New(name As String)
        Me._name = name
        InitializeTextureCache()
    End Sub

    Public Sub New()
        ' Parameterless constructor for serialization
        InitializeTextureCache()
    End Sub

    Private Sub InitializeTextureCache()
        If _textureCache Is Nothing Then
            _textureCache = New Dictionary(Of String, Image)
        End If
    End Sub

    ' Called after deserialization
    <OnDeserialized>
    Private Sub OnDeserialized(context As StreamingContext)
        InitializeTextureCache()
        _isDeserializing = False
    End Sub

    Protected Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    ' Texture loading methods
    Public Function LoadTexture(texturePath As String) As Image
        If _disposed Then
            Throw New ObjectDisposedException(GetType(Material).Name)
        End If

        If String.IsNullOrEmpty(texturePath) Then
            Return Nothing
        End If

        If Not File.Exists(texturePath) Then
            Throw New FileNotFoundException($"Texture file not found: {texturePath}")
        End If

        ' Ensure cache is initialized
        If _textureCache Is Nothing Then
            InitializeTextureCache()
        End If

        ' Check cache first
        If _textureCache.ContainsKey(texturePath) Then
            Return _textureCache(texturePath)
        End If

        Try
            Dim texture As Image = Image.FromFile(texturePath)
            _textureCache(texturePath) = texture
            Return texture
        Catch ex As Exception
            Throw New InvalidOperationException($"Failed to load texture: {texturePath}", ex)
        End Try
    End Function

    ' Unload specific texture
    Public Sub UnloadTexture(texturePath As String)
        If _disposed OrElse String.IsNullOrEmpty(texturePath) Then Return

        If _textureCache IsNot Nothing AndAlso _textureCache.ContainsKey(texturePath) Then
            Try
                Dim texture = _textureCache(texturePath)
                texture?.Dispose()
            Finally
                _textureCache.Remove(texturePath)
            End Try
        End If
    End Sub

    ' Unload all currently loaded textures
    Public Sub UnloadAllTextures()
        If _disposed Then Return

        ClearTextureCache()
    End Sub

    ' Unload specific texture by type
    Public Sub UnloadTextureByType(textureType As TextureType)
        Select Case textureType
            Case TextureType.Diffuse
                UnloadTexture(DiffuseTexture)
            Case TextureType.Normal
                UnloadTexture(NormalTexture)
            Case TextureType.Specular
                UnloadTexture(SpecularTexture)
            Case TextureType.Albedo
                UnloadTexture(AlbedoTexture)
            Case TextureType.Metallic
                UnloadTexture(MetallicTexture)
            Case TextureType.Roughness
                UnloadTexture(RoughnessTexture)
            Case TextureType.AmbientOcclusion
                UnloadTexture(AOTexture)
            Case TextureType.Height
                UnloadTexture(HeightTexture)
        End Select
    End Sub

    Public Function LoadAlbedoTexture() As Image
        Return LoadTexture(AlbedoTexture)
    End Function

    Public Function LoadMetallicTexture() As Image
        Return LoadTexture(MetallicTexture)
    End Function

    Public Function LoadRoughnessTexture() As Image
        Return LoadTexture(RoughnessTexture)
    End Function

    Public Function LoadAOTexture() As Image
        Return LoadTexture(AOTexture)
    End Function

    Public Function LoadHeightTexture() As Image
        Return LoadTexture(HeightTexture)
    End Function

    Public Function LoadDiffuseTexture() As Image
        Return LoadTexture(DiffuseTexture)
    End Function

    Public Function LoadNormalTexture() As Image
        Return LoadTexture(NormalTexture)
    End Function

    Public Function LoadSpecularTexture() As Image
        Return LoadTexture(SpecularTexture)
    End Function

    ' Load all textures at once
    Public Sub LoadAllTextures()
        If _disposed Then
            Throw New ObjectDisposedException(GetType(Material).Name)
        End If

        If UsePBR Then
            LoadAlbedoTexture()
            LoadMetallicTexture()
            LoadRoughnessTexture()
            LoadAOTexture()
            LoadHeightTexture()
        Else
            LoadDiffuseTexture()
            LoadSpecularTexture()
        End If

        LoadNormalTexture()
    End Sub

    ' Clear texture cache
    Public Sub ClearTextureCache()
        If _textureCache IsNot Nothing Then
            ' Create a copy of keys to avoid modification during enumeration
            Dim keys = _textureCache.Keys.ToList()

            For Each key In keys
                Try
                    Dim texture = _textureCache(key)
                    texture?.Dispose()
                Catch ex As Exception
                    ' Log error if needed
                End Try
            Next

            _textureCache.Clear()
        End If
    End Sub

    ' Get loaded texture count
    Public Function GetLoadedTextureCount() As Integer
        If _textureCache IsNot Nothing Then
            Return _textureCache.Count
        End If
        Return 0
    End Function

    ' Get memory usage estimate
    Public Function GetEstimatedMemoryUsage() As Long
        Dim totalBytes As Long = 0

        If _textureCache IsNot Nothing Then
            For Each kvp In _textureCache
                If kvp.Value IsNot Nothing Then
                    Try
                        totalBytes += kvp.Value.Width * kvp.Value.Height * 4 ' Assume 4 bytes per pixel
                    Catch
                        ' Ignore errors
                    End Try
                End If
            Next
        End If

        Return totalBytes
    End Function

    ' Get all loaded texture paths
    Public Function GetLoadedTexturePaths() As List(Of String)
        If _textureCache IsNot Nothing Then
            Return _textureCache.Keys.ToList()
        End If
        Return New List(Of String)
    End Function

    ' Validate texture paths
    Public Function ValidateTexturePaths() As List(Of String)
        Dim missingTextures As New List(Of String)

        If Not String.IsNullOrEmpty(DiffuseTexture) AndAlso Not File.Exists(DiffuseTexture) Then
            missingTextures.Add($"Diffuse: {DiffuseTexture}")
        End If

        If Not String.IsNullOrEmpty(NormalTexture) AndAlso Not File.Exists(NormalTexture) Then
            missingTextures.Add($"Normal: {NormalTexture}")
        End If

        If Not String.IsNullOrEmpty(SpecularTexture) AndAlso Not File.Exists(SpecularTexture) Then
            missingTextures.Add($"Specular: {SpecularTexture}")
        End If

        If UsePBR Then
            If Not String.IsNullOrEmpty(AlbedoTexture) AndAlso Not File.Exists(AlbedoTexture) Then
                missingTextures.Add($"Albedo: {AlbedoTexture}")
            End If

            If Not String.IsNullOrEmpty(MetallicTexture) AndAlso Not File.Exists(MetallicTexture) Then
                missingTextures.Add($"Metallic: {MetallicTexture}")
            End If

            If Not String.IsNullOrEmpty(RoughnessTexture) AndAlso Not File.Exists(RoughnessTexture) Then
                missingTextures.Add($"Roughness: {RoughnessTexture}")
            End If

            If Not String.IsNullOrEmpty(AOTexture) AndAlso Not File.Exists(AOTexture) Then
                missingTextures.Add($"AO: {AOTexture}")
            End If

            If Not String.IsNullOrEmpty(HeightTexture) AndAlso Not File.Exists(HeightTexture) Then
                missingTextures.Add($"Height: {HeightTexture}")
            End If
        End If

        Return missingTextures
    End Function

    Public Function Clone() As Material
        Return New Material(Me.Name & "_Copy") With {
            .DiffuseColor = Me.DiffuseColor,
            .AmbientColor = Me.AmbientColor,
            .SpecularColor = Me.SpecularColor,
            .EmissiveColor = Me.EmissiveColor,
            .Shininess = Me.Shininess,
            .Opacity = Me.Opacity,
            .Reflection = Me.Reflection,
            .Refraction = Me.Refraction,
            .BumpStrength = Me.BumpStrength,
            .DiffuseTexture = Me.DiffuseTexture,
            .NormalTexture = Me.NormalTexture,
            .SpecularTexture = Me.SpecularTexture,
            .UsePBR = Me.UsePBR,
            .Albedo = Me.Albedo,
            .Metallic = Me.Metallic,
            .Roughness = Me.Roughness,
            .AO = Me.AO,
            .AlbedoTexture = Me.AlbedoTexture,
            .MetallicTexture = Me.MetallicTexture,
            .RoughnessTexture = Me.RoughnessTexture,
            .AOTexture = Me.AOTexture,
            .HeightTexture = Me.HeightTexture,
            .TwoSided = Me.TwoSided,
            .CastShadows = Me.CastShadows,
            .ReceiveShadows = Me.ReceiveShadows,
            .Wireframe = Me.Wireframe,
            .FacetedShading = Me.FacetedShading
        }
    End Function

    ' IDisposable implementation
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not _disposed Then
            If disposing Then
                ' Dispose managed resources
                ClearTextureCache()
                _textureCache = Nothing
            End If

            _disposed = True
        End If
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub

    ' Check if disposed
    Public ReadOnly Property IsDisposed As Boolean
        Get
            Return _disposed
        End Get
    End Property
End Class

' Texture type enumeration
Public Enum TextureType
    Diffuse
    Normal
    Specular
    Albedo
    Metallic
    Roughness
    AmbientOcclusion
    Height
End Enum

<Serializable>
Public Class MaterialLibrary
    Implements IDisposable

    Public Property Materials As List(Of Material)
    Private _disposed As Boolean = False

    Public Sub New()
        Materials = New List(Of Material)
    End Sub

    ' Add material to library
    Public Sub AddMaterial(material As Material)
        If material IsNot Nothing Then
            Materials.Add(material)
        End If
    End Sub

    ' Remove material and dispose it
    Public Sub RemoveMaterial(material As Material)
        If material IsNot Nothing AndAlso Materials.Contains(material) Then
            Materials.Remove(material)
            material.Dispose()
        End If
    End Sub

    ' Clear all materials
    Public Sub Clear()
        For Each material In Materials
            material?.Dispose()
        Next
        Materials.Clear()
    End Sub

    ' Dispose all materials in the library
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not _disposed Then
            If disposing Then
                If Materials IsNot Nothing Then
                    For Each material In Materials
                        material?.Dispose()
                    Next
                    Materials.Clear()
                    Materials = Nothing
                End If
            End If

            _disposed = True
        End If
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub
End Class