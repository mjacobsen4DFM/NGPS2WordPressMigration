﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'This class was auto-generated by the StronglyTypedResourceBuilder
    'class via a tool like ResGen or Visual Studio.
    'To add or remove a member, edit your .ResX file then rerun ResGen
    'with the /str option, or rebuild your VS project.
    '''<summary>
    '''  A strongly-typed resource class, for looking up localized strings, etc.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>  _
    Friend Class wpResources
        
        Private Shared resourceMan As Global.System.Resources.ResourceManager
        
        Private Shared resourceCulture As Global.System.Globalization.CultureInfo
        
        <Global.System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>  _
        Friend Sub New()
            MyBase.New
        End Sub
        
        '''<summary>
        '''  Returns the cached ResourceManager instance used by this class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Shared ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("BusinessRule.wpResources", GetType(wpResources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Overrides the current thread's CurrentUICulture property for all
        '''  resource lookups using this strongly typed resource class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Shared Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to  &quot;id&quot;:[{0}] .
        '''</summary>
        Friend Shared ReadOnly Property capJson() As String
            Get
                Return ResourceManager.GetString("capJson", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to    &quot;post_id&quot;:&quot;{0}&quot;,
        '''   &quot;postlocation&quot;:&quot;{1}&quot;,
        '''   &quot;name&quot;:&quot;{2}&quot;,
        '''   &quot;source&quot;:&quot;{3}&quot;,
        '''   &quot;mimetype&quot;:&quot;{4}&quot;,
        '''   &quot;caption&quot;:&quot;{5}&quot;,
        '''   &quot;featured&quot;:&quot;{6}&quot;,
        '''   &quot;author&quot;:&quot;{7}&quot;,
        '''   &quot;date&quot;:&quot;{8}&quot;.
        '''</summary>
        Friend Shared ReadOnly Property imageJson() As String
            Get
                Return ResourceManager.GetString("imageJson", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to &quot;key&quot;:&quot;{0}&quot;, 
        '''&quot;value&quot;:&quot;{1}&quot;.
        '''</summary>
        Friend Shared ReadOnly Property metaJson() As String
            Get
                Return ResourceManager.GetString("metaJson", resourceCulture)
            End Get
        End Property
    End Class
End Namespace
