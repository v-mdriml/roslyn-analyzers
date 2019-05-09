﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PropertySetAnalysis)]
    public class DoNotUseInsecureDeserializerNetDataContractSerializerWithoutBinderTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor BinderNotSetRule = DoNotUseInsecureDeserializerNetDataContractSerializerWithoutBinder.RealBinderDefinitelyNotSetDescriptor;

        private static readonly DiagnosticDescriptor BinderMaybeNotSetRule = DoNotUseInsecureDeserializerNetDataContractSerializerWithoutBinder.RealBinderMaybeNotSetDescriptor;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerNetDataContractSerializerWithoutBinder();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureDeserializerNetDataContractSerializerWithoutBinder();
        }

        protected void VerifyCSharpWithMyBinderDefined(string source, params DiagnosticResult[] expected)
        {
            string myBinderCSharpSourceCode = @"
using System;
using System.Runtime.Serialization;

namespace Blah
{
    public class MyBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            throw new NotImplementedException();
        }
    }

    public class SomeOtherSerializer
    {
        public object Deserialize(byte[] bytes)
        {
            return null;
        }
    }
}
            ";

            this.VerifyCSharp(
                new[] { source, myBinderCSharpSourceCode }.ToFileAndSource(),
                expected);
        }

        [Fact]
        public void DocSample1_CSharp_Violation_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[DataContract]
public class BookRecord
{
    [DataMember]
    public string Title { get; set; }

    [DataMember]
    public string Author { get; set; }

    [DataMember]
    public int PageCount { get; set; }

    [DataMember]
    public AisleLocation Location { get; set; }
}

[DataContract]
public class AisleLocation
{
    [DataMember]
    public char Aisle { get; set; }

    [DataMember]
    public byte Shelf { get; set; }
}

public class ExampleClass
{
    public BookRecord DeserializeBookRecord(byte[] bytes)
    {
        NetDataContractSerializer serializer = new NetDataContractSerializer();
        using (MemoryStream ms = new MemoryStream(bytes))
        {
            return (BookRecord) serializer.Deserialize(ms);
        }
    }
}",
                GetCSharpResultAt(39, 33, BinderNotSetRule, "object NetDataContractSerializer.Deserialize(Stream stream)"));
        }

        [Fact]
        public void DocSample1_VB_Violation_Diagnostic()
        {
            VerifyBasic(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

<DataContract()>
Public Class BookRecord
    <DataMember()>
    Public Property Title As String

    <DataMember()>
    Public Property Author As String

    <DataMember()>
    Public Property Location As AisleLocation
End Class

<DataContract()>
Public Class AisleLocation
    <DataMember()>
    Public Property Aisle As Char

    <DataMember()>
    Public Property Shelf As Byte
End Class

Public Class ExampleClass
    Public Function DeserializeBookRecord(bytes As Byte()) As BookRecord
        Dim serializer As NetDataContractSerializer = New NetDataContractSerializer()
        Using ms As MemoryStream = New MemoryStream(bytes)
            Return CType(serializer.Deserialize(ms), BookRecord)
        End Using
    End Function
End Class",
                GetBasicResultAt(31, 26, BinderNotSetRule, "Function NetDataContractSerializer.Deserialize(stream As Stream) As Object"));
        }

        [Fact]
        public void DocSample1_CSharp_Solution_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Runtime.Serialization;

public class BookRecordSerializationBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        // One way to discover expected types is through testing deserialization
        // of **valid** data and logging the types used.

        ////Console.WriteLine($""BindToType('{assemblyName}', '{typeName}')"");

        if (typeName == ""BookRecord"" || typeName == ""AisleLocation"")
        {
            return null;
        }
        else
        {
            throw new ArgumentException(""Unexpected type"", nameof(typeName));
        }
    }
}

[DataContract]
public class BookRecord
{
    [DataMember]
    public string Title { get; set; }

    [DataMember]
    public string Author { get; set; }

    [DataMember]
    public int PageCount { get; set; }

    [DataMember]
    public AisleLocation Location { get; set; }
}

[DataContract]
public class AisleLocation
{
    [DataMember]
    public char Aisle { get; set; }

    [DataMember]
    public byte Shelf { get; set; }
}

public class ExampleClass
{
    public BookRecord DeserializeBookRecord(byte[] bytes)
    {
        NetDataContractSerializer serializer = new NetDataContractSerializer();
        serializer.Binder = new BookRecordSerializationBinder();
        using (MemoryStream ms = new MemoryStream(bytes))
        {
            return (BookRecord) serializer.Deserialize(ms);
        }
    }
}");
        }

        [Fact]
        public void DocSample1_VB_Solution_NoDiagnostic()
        {
            VerifyBasic(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Public Class BookRecordSerializationBinder
    Inherits SerializationBinder

    Public Overrides Function BindToType(assemblyName As String, typeName As String) As Type
        ' One way to discover expected types is through testing deserialization
        ' of **valid** data and logging the types used.

        'Console.WriteLine($""BindToType('{assemblyName}', '{typeName}')"")

        If typeName = ""BinaryFormatterVB.BookRecord"" Or typeName = ""BinaryFormatterVB.AisleLocation"" Then
            Return Nothing
        Else
            Throw New ArgumentException(""Unexpected type"", NameOf(typeName))
        End If
    End Function
End Class

<DataContract()>
Public Class BookRecord
    <DataMember()>
    Public Property Title As String

    <DataMember()>
    Public Property Author As String

    <DataMember()>
    Public Property Location As AisleLocation
End Class

<DataContract()>
Public Class AisleLocation
    <DataMember()>
    Public Property Aisle As Char

    <DataMember()>
    Public Property Shelf As Byte
End Class

Public Class ExampleClass
    Public Function DeserializeBookRecord(bytes As Byte()) As BookRecord
        Dim serializer As NetDataContractSerializer = New NetDataContractSerializer()
        serializer.Binder = New BookRecordSerializationBinder()
        Using ms As MemoryStream = New MemoryStream(bytes)
            Return CType(serializer.Deserialize(ms), BookRecord)
        End Using
    End Function
End Class");
        }

        [Fact]
        public void DocSample2_CSharp_Violation_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[DataContract]
public class BookRecord
{
    [DataMember]
    public string Title { get; set; }

    [DataMember]
    public string Author { get; set; }

    [DataMember]
    public int PageCount { get; set; }

    [DataMember]
    public AisleLocation Location { get; set; }
}

[DataContract]
public class AisleLocation
{
    [DataMember]
    public char Aisle { get; set; }

    [DataMember]
    public byte Shelf { get; set; }
}

public class ExampleClass
{
    public NetDataContractSerializer Serializer { get; set; }

    public BookRecord DeserializeBookRecord(byte[] bytes)
    {
        using (MemoryStream ms = new MemoryStream(bytes))
        {
            return (BookRecord) this.Serializer.Deserialize(ms);
        }
    }
}",
                GetCSharpResultAt(40, 33, BinderMaybeNotSetRule, "object NetDataContractSerializer.Deserialize(Stream stream)"));
        }

        [Fact]
        public void DocSample2_VB_Violation_Diagnostic()
        {
            VerifyBasic(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

<DataContract()>
Public Class BookRecord
    <DataMember()>
    Public Property Title As String

    <DataMember()>
    Public Property Author As String

    <DataMember()>
    Public Property Location As AisleLocation
End Class

<DataContract()>
Public Class AisleLocation
    <DataMember()>
    Public Property Aisle As Char

    <DataMember()>
    Public Property Shelf As Byte
End Class

Public Class ExampleClass
    Public Property Serializer As NetDataContractSerializer

    Public Function DeserializeBookRecord(bytes As Byte()) As BookRecord
        Using ms As MemoryStream = New MemoryStream(bytes)
            Return CType(Me.Serializer.Deserialize(ms), BookRecord)
        End Using
    End Function
End Class",
                GetBasicResultAt(32, 26, BinderMaybeNotSetRule, "Function NetDataContractSerializer.Deserialize(stream As Stream) As Object"));
        }

        [Fact]
        public void Deserialize_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public object TestMethod(byte[] bytes)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            return serializer.Deserialize(new MemoryStream(bytes));
        }
    }
}",
            GetCSharpResultAt(12, 20, BinderNotSetRule, "object NetDataContractSerializer.Deserialize(Stream stream)"));
        }

        // Ideally, we'd detect that serializer.Binder is always null.
        [Fact]
        public void DeserializeWithInstanceField_Diagnostic_NotIdeal()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        NetDataContractSerializer serializer = new NetDataContractSerializer();

        public object TestMethod(byte[] bytes)
        {
            return this.serializer.Deserialize(new MemoryStream(bytes));
        }
    }
}",
            GetCSharpResultAt(13, 20, BinderMaybeNotSetRule, "object NetDataContractSerializer.Deserialize(Stream stream)"));
        }

        [Fact]
        public void Deserialize_BinderMaybeSet_Diagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public object TestMethod(byte[] bytes)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            if (Environment.MachineName.StartsWith(""a""))
            {
                serializer.Binder = new MyBinder();
            }

            return serializer.Deserialize(new MemoryStream(bytes));
        }
    }
}",
            GetCSharpResultAt(18, 20, BinderMaybeNotSetRule, "object NetDataContractSerializer.Deserialize(Stream stream)"));
        }

        [Fact]
        public void Deserialize_BinderSet_NoDiagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public object TestMethod(byte[] bytes)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            serializer.Binder = new MyBinder();
            return serializer.Deserialize(new MemoryStream(bytes));
        }
    }
}");
        }

        [Fact]
        public void TwoDeserializersOneBinderOnFirst_Diagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public object TestMethod(byte[] bytes1, byte[] bytes2)
        {
            if (Environment.GetEnvironmentVariable(""USEFIRST"") == ""1"")
            {
                NetDataContractSerializer bf = new NetDataContractSerializer();
                bf.Binder = new MyBinder();
                return bf.Deserialize(new MemoryStream(bytes1));
            }
            else
            {
                return new NetDataContractSerializer().Deserialize(new MemoryStream(bytes2));
            }
        }
    }
}",
                GetCSharpResultAt(20, 24, BinderNotSetRule, "object NetDataContractSerializer.Deserialize(Stream stream)"));
        }

        [Fact]
        public void TwoDeserializersOneBinderOnSecond_Diagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public object TestMethod(byte[] bytes1, byte[] bytes2)
        {
            if (Environment.GetEnvironmentVariable(""USEFIRST"") == ""1"")
            {
                return new NetDataContractSerializer().Deserialize(new MemoryStream(bytes1));
            }
            else
            {
                return (new NetDataContractSerializer() { Binder = new MyBinder() }).Deserialize(new MemoryStream(bytes2));
            }
        }
    }
}",
                GetCSharpResultAt(14, 24, BinderNotSetRule, "object NetDataContractSerializer.Deserialize(Stream stream)"));

        }

        [Fact]
        public void TwoDeserializersNoBinder_Diagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public object TestMethod(byte[] bytes1, byte[] bytes2)
        {
            if (Environment.GetEnvironmentVariable(""USEFIRST"") == ""1"")
            {
                return new NetDataContractSerializer().Deserialize(new MemoryStream(bytes1));
            }
            else
            {
                return new NetDataContractSerializer().Deserialize(new MemoryStream(bytes2));
            }
        }
    }
}",
                GetCSharpResultAt(14, 24, BinderNotSetRule, "object NetDataContractSerializer.Deserialize(Stream stream)"),
                GetCSharpResultAt(18, 24, BinderNotSetRule, "object NetDataContractSerializer.Deserialize(Stream stream)"));

        }

        [Fact]
        public void BinderSetInline_NoDiagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public object TestMethod(byte[] bytes)
        {
            return (new NetDataContractSerializer() { Binder = new MyBinder() }).Deserialize(new MemoryStream(bytes));
        }
    }
}");
        }

        [Fact]
        public void Serialize_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public byte[] S(object o)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, o);
            return ms.ToArray();
        }
    }
}");
        }


        [Fact]
        public void Deserialize_InvokedAsDelegate_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        delegate object DeserializeDelegate(Stream s);

        public object DeserializeWithDelegate(byte[] bytes)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            DeserializeDelegate del = serializer.Deserialize;
            return del(new MemoryStream(bytes));
        }
    }
}",
                GetCSharpResultAt(15, 20, BinderNotSetRule, "object NetDataContractSerializer.Deserialize(Stream stream)"));
        }

        [Fact]
        public void ReadObject_Stream_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public object TestMethod(byte[] bytes)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            return serializer.ReadObject(new MemoryStream(bytes));
        }
    }
}",
            GetCSharpResultAt(12, 20, BinderNotSetRule, "object XmlObjectSerializer.ReadObject(Stream stream)"));
        }

        [Fact]
        public void ReadObject_Stream_BinderMaybeSet_Diagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public object TestMethod(byte[] bytes)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            if (Environment.MachineName.StartsWith(""a""))
            {
                serializer.Binder = new MyBinder();
            }

            return serializer.ReadObject(new MemoryStream(bytes));
        }
    }
}",
            GetCSharpResultAt(18, 20, BinderMaybeNotSetRule, "object XmlObjectSerializer.ReadObject(Stream stream)"));
        }

        [Fact]
        public void ReadObject_Stream_BinderSet_NoDiagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        public object TestMethod(byte[] bytes)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            serializer.Binder = new MyBinder();
            return serializer.ReadObject(new MemoryStream(bytes));
        }
    }
}");
        }

        [Fact]
        public void ReadObject_Stream_InvokedAsDelegate_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;

namespace Blah
{
    public class Program
    {
        delegate object DeserializeDelegate(Stream s);

        public object DeserializeWithDelegate(byte[] bytes)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            DeserializeDelegate del = serializer.ReadObject;
            return del(new MemoryStream(bytes));
        }
    }
}",
                GetCSharpResultAt(15, 20, BinderNotSetRule, "object XmlObjectSerializer.ReadObject(Stream stream)"));
        }

        [Fact]
        public void ReadObject_XmlReader_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Blah
{
    public class Program
    {
        public object TestMethod(XmlReader xmlReader)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            return serializer.ReadObject(xmlReader);
        }
    }
}",
            GetCSharpResultAt(13, 20, BinderNotSetRule, "object NetDataContractSerializer.ReadObject(XmlReader reader)"));
        }

        [Fact]
        public void ReadObject_XmlReader_BinderMaybeSet_Diagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Blah
{
    public class Program
    {
        public object TestMethod(XmlReader xmlReader)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            if (Environment.MachineName.StartsWith(""a""))
            {
                serializer.Binder = new MyBinder();
            }

            return serializer.ReadObject(xmlReader);
        }
    }
}",
            GetCSharpResultAt(19, 20, BinderMaybeNotSetRule, "object NetDataContractSerializer.ReadObject(XmlReader reader)"));
        }

        [Fact]
        public void ReadObject_XmlReader_BinderSet_NoDiagnostic()
        {
            VerifyCSharpWithMyBinderDefined(@"
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Blah
{
    public class Program
    {
        public object TestMethod(XmlReader xmlReader)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            serializer.Binder = new MyBinder();
            return serializer.ReadObject(xmlReader);
        }
    }
}");
        }

        [Fact]
        public void ReadObject_XmlReader_InvokedAsDelegate_Diagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Blah
{
    public class Program
    {
        delegate object DeserializeDelegate(XmlReader r);

        public object DeserializeWithDelegate(XmlReader xmlReader)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            DeserializeDelegate del = serializer.ReadObject;
            return del(xmlReader);
        }
    }
}",
                GetCSharpResultAt(16, 20, BinderNotSetRule, "object NetDataContractSerializer.ReadObject(XmlReader reader)"));
        }
    }
}