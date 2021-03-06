//
// updater.cs: Mono documentation updater
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian, Inc.
//
//

using System;
using System.Globalization;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

class Match
{
        string signature;
        bool found;
        MemberInfo member;
        XmlNode node;

        public Match (string signature, bool found)
        {
                this.signature = signature;
                this.found = found;
        }

        public MemberInfo Member {
                get { return member; }
                set { member = value; }
        }

        public XmlNode Node {
                get { return node; }
                set { node = value; }
        }

        public string Signature {
                get { return signature; }
                set { signature = value; }
        }

        public bool Found {
                get { return found; }
                set { found = value; }
        }
}

public enum MemberType {
        Method,
        Property,
        Constructor,
        Event,
        Field,
}

class Updater {

        static string EmptyString = "To be added";
        static DirectoryInfo root_directory;
        static bool overwrite = false;
        static string Namespace;
        static bool remove = false;

        static void Main (string [] args)
        {
                if (args.Length < 1) {
                        Console.WriteLine ("Usage: updater <assembly> [-o <output dir>] [-f]");
                        return;
                }

                if (args.Length >= 3 && args [1] == "-o" ) {
                        if (args.Length == 4 && args [3] == "-f") {
                                remove = true;
                                overwrite = true;
                        }

                        root_directory = new DirectoryInfo (args [2]);

                        if (!root_directory.Exists)
                                root_directory.Create ();
                } else
                        root_directory = new DirectoryInfo (Directory.GetCurrentDirectory ());

                string assembly = args [0];

                Generate (assembly);
        }

        static void Generate (string assembly)
        {
                Assembly a = Assembly.LoadFrom (assembly);

                if (a == null)
                        throw new NullReferenceException ("Null assembly");

                int count = 0;
                foreach (Type t in a.GetTypes ()) {
                        Namespace = t.Namespace;
                        Generate (t, assembly, t.Namespace, ref count);
                }

                Console.WriteLine (String.Format ("{0}: Wrote {1} files.\n", assembly, count));
        }

        static void Generate (Type type, string assembly, string ns, ref int count)
        {
                if (ns == null)
                        return;

                DirectoryInfo directory;

                try {
                        directory = root_directory.CreateSubdirectory (ns);

                } catch (IOException) {
                        directory = new DirectoryInfo (Path.Combine (root_directory.FullName, ns));
                }

                string filename = Path.Combine (directory.FullName, GetName (type) + ".xml");
                XmlDocument document;

                if (File.Exists (filename)) {
                        XmlDocument existing = new XmlDocument ();
                        Stream stream = File.OpenRead (filename);
                        existing.Load (stream);
			stream.Close ();
                        document  = Compare (type, existing);

                } else {

                        document = Generate (type);
                }

                if (document == null) {
                        return;

                } else {
                        count ++;
                        XmlTextWriter writer = new XmlTextWriter (filename, Encoding.ASCII);
                        writer.Formatting = Formatting.Indented;
                        document.WriteContentTo (writer);
                        writer.Flush ();
                }
        }

        static FieldInfo [] GetAllFields (Type t)
        {
                BindingFlags static_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Static;
                BindingFlags instance_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Instance;

                FieldInfo [] static_members = t.GetFields (static_flag);
                FieldInfo [] instance_members = t.GetFields (instance_flag);

                if (static_members == null && instance_members == null)
                        return null;

                FieldInfo [] all_members = new FieldInfo [static_members.Length + instance_members.Length];
                static_members.CopyTo (all_members, 0); // copy all static members
                instance_members.CopyTo (all_members, static_members.Length); // copy all instance members

                return all_members;
        }

        static MethodInfo [] GetAllMethods (Type t)
        {
                BindingFlags static_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Static;
                BindingFlags instance_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Instance;

                MethodInfo [] static_members = t.GetMethods (static_flag);
                MethodInfo [] instance_members = t.GetMethods (instance_flag);

                if (static_members == null && instance_members == null)
                        return null;

                MethodInfo [] all_members = new MethodInfo [static_members.Length + instance_members.Length];
                static_members.CopyTo (all_members, 0); // copy all static members
                instance_members.CopyTo (all_members, static_members.Length); // copy all instance members

                return all_members;
        }

        static PropertyInfo [] GetAllProperties (Type t)
        {
                BindingFlags static_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Static;
                BindingFlags instance_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Instance;

                PropertyInfo [] static_members = t.GetProperties (static_flag);
                PropertyInfo [] instance_members = t.GetProperties (instance_flag);

                if (static_members == null && instance_members == null)
                        return null;

                PropertyInfo [] all_members = new PropertyInfo [static_members.Length + instance_members.Length];
                static_members.CopyTo (all_members, 0); // copy all static members
                instance_members.CopyTo (all_members, static_members.Length); // copy all instance members

                return all_members;
        }

        static EventInfo [] GetAllEvents (Type t)
        {
                BindingFlags static_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Static;
                BindingFlags instance_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Instance;

                EventInfo [] static_members = t.GetEvents (static_flag);
                EventInfo [] instance_members = t.GetEvents (instance_flag);

                if (static_members == null && instance_members == null)
                        return null;

                EventInfo [] all_members = new EventInfo [static_members.Length + instance_members.Length];
                static_members.CopyTo (all_members, 0); // copy all static members
                instance_members.CopyTo (all_members, static_members.Length); // copy all instance members

                return all_members;
        }

        static ConstructorInfo [] GetAllConstructors (Type t)
        {
                BindingFlags static_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Static;
                BindingFlags instance_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Instance;

                ConstructorInfo [] static_members = t.GetConstructors (static_flag);
                ConstructorInfo [] instance_members = t.GetConstructors (instance_flag);

                if (static_members == null && instance_members == null)
                        return null;

                ConstructorInfo [] all_members = new ConstructorInfo [static_members.Length + instance_members.Length];
                static_members.CopyTo (all_members, 0); // copy all static members
                instance_members.CopyTo (all_members, static_members.Length); // copy all instance members

                return all_members;
        }
        static XmlDocument Compare (Type t, XmlDocument existing)
        {
                BindingFlags static_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Static;
                BindingFlags instance_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Instance;

                Console.WriteLine (t.FullName);

                //
                // if it's a delegate, check that and move on.
                //
                bool isDelagate;

                if (!t.IsAbstract && typeof (System.Delegate).IsAssignableFrom (t)) {
                        string node_signature = GetNodeSignature (existing);
                        string del_signature = AddTypeSignature (t, out isDelagate);
                        if (isDelagate) {
                                if (node_signature == del_signature)
                                        return existing;
                                else {
                                        SetTypeSignature (existing, del_signature);
                                        return existing;
                                }
                        }
                }

                // Check everything
                FieldInfo [] fields = GetAllFields (t);
                Compare (existing, MemberType.Field,
                                fields, GetNodesOfType (existing, "Field"));

                PropertyInfo [] properties = GetAllProperties (t);
                Compare (existing, MemberType.Property,
                                properties, GetNodesOfType (existing, "Property"));

                EventInfo [] events = GetAllEvents (t);
                Compare (existing, MemberType.Event,
                                events, GetNodesOfType (existing, "Event"));

                MethodInfo [] methods = GetAllMethods (t);
                Compare (existing, MemberType.Method,
                                methods, GetNodesOfType (existing, "Method"));

                ConstructorInfo [] constructors = GetAllConstructors (t);
                Compare (existing, MemberType.Constructor,
                                constructors, GetNodesOfType (existing, "Constructor"));

                return existing;
        }

        static void Compare (XmlDocument document, MemberType member_type, MemberInfo [] members, XmlNodeList nodes)
        {
                if ((members == null || members.Length == 0) && nodes == null)
                        return;

                int members_count = ((members == null) ? 0 :members.Length);
                int nodes_count = ((nodes == null) ? 0 : nodes.Count);

                //
                // There are no existing nodes, we generate all members
                //
                if (members_count > 0 && nodes == null) {
                        GenerateMembers (members, document);
                        return;
                }

                //
                // If we have no existing members, we deprecate all nodes
                //
                if (nodes_count > 0 && members_count == 0 ) {
                        DeprecateNodes (document, nodes);
                        return;
                }

                ArrayList types_list;
                Match [] nodes_list = MakeNodesList (nodes);

                switch (member_type) {
                case MemberType.Field:
                        types_list = MakeFieldList (members);
                        break;
                case MemberType.Property:
                        types_list = MakePropertyList (members);
                        break;
                case MemberType.Method:
                        types_list = MakeMethodList (members);
                        break;
                case MemberType.Constructor:
                        types_list = MakeConstructorList (members);
                        break;
                case MemberType.Event:
                        types_list = MakeEventList (members);
                        break;
                default:
                        throw new ArgumentException ();
                }

//              Console.Write ("Types list: ");
//              foreach (Match m in types_list)
//                      Console.Write (m.Member.Name + " ");

//              Console.Write ("\nNodes list: ");
//              foreach (Match n in nodes_list)
//                      Console.Write (GetNodeName (n.Node) + " ");

                //
                // Look for stuff that we can match between the type and the document
                //
                foreach (object obj in types_list) {
                        foreach (Match node in nodes_list) {
                                Match type = (Match) obj;
                                if (node.Signature != type.Signature) {
                                        continue;
                                } else {
                                        node.Found = true;
                                        type.Found = true;
                                }
                        }
                }

                //
                // Go thru each item found in the type,
                // If it's not found, then generate it in the document
                //
                foreach (object o in types_list) {
                        Match match = (Match) o;
                        if (match.Found) {
                                continue;
                        } else {
                                XmlElement element = GetMembersElement (document);
                                switch (member_type) {
                                case MemberType.Constructor:
                                        AddConstructor (element, (ConstructorInfo) match.Member);
                                        break;
                                case MemberType.Event:
                                        AddEvent (element, (EventInfo) match.Member);
                                        break;
                                case MemberType.Method:
                                        AddMethod (element, (MethodInfo) match.Member);
                                        break;
                                case MemberType.Property:
                                        AddProperty (element, (PropertyInfo) match.Member);
                                        break;
                                case MemberType.Field:
                                        AddField (element, (FieldInfo) match.Member);
                                        break;
                                default:
                                        throw new Exception ();
                                }
                        }
                }

                //
                // Go thru each item in the node list,
                // If there's no corresponding MemberInfo in the type,
                // then mark it as deprecated and print a warning.
                //
                foreach (Match match in nodes_list) {
                        if (match.Found)
                                continue;
                        else {
                                Console.WriteLine ("{0} cannot be found, we'll deprecate it.", GetNodeName (match.Node));
                                DeprecateNode (document, match.Node);
                        }
                }
        }

        static void GenerateMembers (MemberInfo [] members, XmlDocument document)
        {
                XmlElement element = GetMembersElement (document);
                foreach (MemberInfo member in members) {
                        if (member is FieldInfo) {
                                AddField (element, (FieldInfo) member);
                                continue;

                        } else if (member is PropertyInfo) {
                                AddProperty (element, (PropertyInfo) member);
                                continue;

                        } else if (member is ConstructorInfo) {
                                AddConstructor (element, (ConstructorInfo) member);
                                continue;

                        } else if (member is MethodInfo) {
                                AddMethod (element, (MethodInfo) member);
                                continue;

                        } else if (member is EventInfo) {
                                AddEvent (element, (EventInfo) member);
                                continue;

                        } else {
                                Console.WriteLine ("{0} is of type {1}", member.Name, member.GetType ());
                                throw new Exception ("Can't generate member.");
                        }
                }
        }

        static void DeprecateNodes (XmlDocument document, XmlNodeList nodes)
        {
                foreach (XmlNode node in nodes)
                        DeprecateNode (document, node);
        }

        static void DeprecateNode (XmlDocument document, XmlNode node)
        {
                Console.WriteLine ("Warning! {0} cannot be found, marking it as deprecated.", GetNodeName (node));
                ((XmlElement) node).SetAttribute ("Deprecated", "true");

                if (remove) {
                        XmlNode members = document.SelectSingleNode ("/Type/Members");
                        node.RemoveAll ();
                        members.RemoveChild (node);
                }
        }

        static ArrayList MakeMethodList (MemberInfo [] members)
        {
                if (members.Length == 0)
                        return null;

                MethodInfo [] methods = (MethodInfo []) members;
                ArrayList list = new ArrayList ();

                foreach (MethodInfo method in methods) {

                        // Filter out methods that are also properties
                        if (method.IsSpecialName)
                                continue;

                        // Filter out methods from events
                        if (method.Name.StartsWith ("add_") || method.Name.StartsWith ("remove_"))
                                continue;

                        string signature = AddMethodSignature (method);

                        if (signature == null)
                                continue;

                        Match m = new Match (signature, false);
                        m.Member = method;
                        list.Add (m);
                }

                return list;
        }

        static string GetNodeSignature (XmlDocument document)
        {
                XmlElement type_signature = (XmlElement) document.SelectSingleNode ("/Type/TypeSignature");
                return type_signature.GetAttribute ("Value");
        }

        static void SetTypeSignature (XmlDocument document, string signature)
        {
                XmlElement type_signature = (XmlElement) document.SelectSingleNode ("/Type/TypeSignature");
                type_signature.SetAttribute ("Value", signature);
        }

        static ArrayList MakePropertyList (MemberInfo [] members)
        {
                if (members.Length == 0)
                        return null;

                PropertyInfo [] properties = (PropertyInfo []) members;
                ArrayList list = new ArrayList ();

                foreach (PropertyInfo property in properties) {

                        string signature = AddPropertySignature (property);

                        if (signature == null)
                                continue;

                        Match m = new Match (signature, false);
                        m.Member = property;
                        list.Add (m);
                }
                return list;
        }

        static ArrayList MakeConstructorList (MemberInfo [] members)
        {
                if (members.Length == 0)
                        return null;

                ConstructorInfo [] constructors = (ConstructorInfo []) members;
                ArrayList list = new ArrayList ();

                foreach (ConstructorInfo constructor in constructors) {
                        string signature = AddConstructorSignature (constructor);

                        // .cctors are not suppose to be visible
                        if (signature == null || constructor.Name == ".cctor")
                                continue;

                        Match m = new Match (signature, false);
                        m.Member = constructor;
                        list.Add (m);
                }
                return list;
        }

        static ArrayList MakeFieldList (MemberInfo [] members)
        {
                if (members.Length == 0)
                        return null;

                FieldInfo [] fields = (FieldInfo []) members;
                ArrayList list = new ArrayList ();

                foreach (FieldInfo field in fields) {
                        string signature = AddFieldSignature (field);

                        if (signature == null)
                                continue;

                        Match m = new Match (signature, false);
                        m.Member = field;
                        list.Add (m);
                }
                return list;
        }

        static ArrayList MakeEventList (MemberInfo [] members)
        {
                if (members.Length == 0)
                        return null;

                EventInfo [] events = (EventInfo []) members;
                ArrayList list = new ArrayList ();

                foreach (EventInfo ev in events) {
                        string signature = AddEventSignature (ev);

                        if (signature == null)
                                continue;

                        Match m = new Match (signature, false);
                        m.Member = ev;
                        list.Add (m);
                }
                return list;
        }

        static Match [] MakeNodesList (XmlNodeList nodes)
        {
                if (nodes.Count == 0)
                        return null;

                Match [] list = new Match [nodes.Count];

                for (int i = 0; i < list.Length; i ++) {
                        Match m = new Match (GetNodeSignature (nodes [i]), false);
                        m.Node = nodes [i];
                        list [i] = m;
                }

                return list;
        }

        static XmlNodeList GetNodesOfType (XmlDocument document, string type)
        {
                string expression = String.Format ("/Type/Members/Member[MemberType=\"{0}\"]", type);
                XmlNodeList nodes = document.SelectNodes (expression);

                if (nodes.Count == 0)
                        return null;

                return nodes;
        }

        static string GetNodeSignature (XmlNode node)
        {
                XmlElement signature = node.SelectSingleNode ("./MemberSignature") as XmlElement;
                return signature.GetAttribute ("Value");
        }

        static string GetNodeName (XmlNode node)
        {
                XmlElement signature = (XmlElement) node;
                return signature.GetAttribute ("MemberName");
        }

        static XmlElement GetMembersElement (XmlDocument document)
        {
                XmlNode node =  document.SelectSingleNode ("/Type/Members");

                return (XmlElement) node;
        }

        static XmlDocument Generate (Type type)
        {
                bool isDelagate;
                string signature = AddTypeSignature (type, out isDelagate);

                if (signature == null)
                        return null;

                XmlDocument document = new XmlDocument ();

                //
                // This is the top level <type> node
                //
                XmlElement type_node = document.CreateElement ("Type");
                document.AppendChild (type_node);
                type_node.SetAttribute ("Name", GetName (type));
                type_node.SetAttribute ("FullName", type.FullName);

                XmlElement type_signature = document.CreateElement ("TypeSignature");
                type_signature.SetAttribute ("Language", "C#");
                type_signature.SetAttribute ("Value", signature);
                type_signature.SetAttribute ("Maintainer", "auto");

                type_node.AppendChild (type_signature);

                //
                // This is for the AssemblyInfo nodes
                //
                XmlElement assembly_info = document.CreateElement ("AssemblyInfo");
                type_node.AppendChild (assembly_info);

                AssemblyName asm_name = type.Assembly.GetName ();

                byte[] public_key = asm_name.GetPublicKey ();
                string key;

                if (public_key == null)
                        key = "null";
                else
                        key = GetKeyString (public_key);

                CultureInfo ci = asm_name.CultureInfo;
                string culture;

                if ((ci == null) || (ci.LCID == CultureInfo.InvariantCulture.LCID))
                        culture = "neutral";
                else
                        culture = ci.ToString ();

                assembly_info.AppendChild (AddElement (document, "AssemblyName", asm_name.Name));
                assembly_info.AppendChild (AddElement (document, "AssemblyPublicKey", key));
                assembly_info.AppendChild (AddElement (document, "AssemblyVersion", asm_name.Version.ToString ()));
                assembly_info.AppendChild (AddElement (document, "AssemblyCulture", culture));

                //
                // Assembly-level Attribute nodes
                //
                XmlElement assembly_attributes = document.CreateElement ("Attributes");
                assembly_info.AppendChild (assembly_attributes);

                object [] attrs = type.Assembly.GetCustomAttributes (false);
                AddAttributes (document, assembly_attributes, attrs);

                //
                // Thread-safety node
                //
                XmlElement thread_safety_statement = document.CreateElement ("ThreadSafetyStatement");
                XmlElement link_element = document.CreateElement ("link");
                link_element.SetAttribute ("location", "node:gtk-sharp/programming/threads");
                link_element.AppendChild (document.CreateTextNode ("Gtk# Thread Programming"));

                thread_safety_statement.AppendChild (
                        document.CreateTextNode ("Gtk# is thread aware, but not thread safe; See the "));
                thread_safety_statement.AppendChild (link_element);
                thread_safety_statement.AppendChild (
                        document.CreateTextNode (" for details."));
                
                type_node.AppendChild (thread_safety_statement);

                //
                // Class-level <Docs> node
                //
                type_node.AppendChild (AddDocsNode (document));

                //
                // <Base>
                //
                XmlElement base_node = document.CreateElement ("Base");
                type_node.AppendChild (base_node);

                if (type.IsEnum)
                        base_node.AppendChild (AddElement (document, "BaseTypeName", "System.Enum"));

                else if (type.IsValueType)
                        base_node.AppendChild (AddElement (document, "BaseTypeName", "System.ValueType"));

                else if (isDelagate)
                        base_node.AppendChild (AddElement (document, "BaseTypeName", "System.Delegate"));

                else if (type.IsClass && type != typeof (object))
                        base_node.AppendChild (AddElement (document, "BaseTypeName", type.BaseType.FullName));

                //
                // <Interfaces>
                //
                XmlElement interfaces = document.CreateElement ("Interfaces");
                type_node.AppendChild (interfaces);
                Type [] ifaces = type.GetInterfaces ();

                if (ifaces != null) {
                        foreach (Type iface in ifaces ) {
                                XmlElement interface_node = document.CreateElement ("Interface");
                                interfaces.AppendChild (interface_node);
                                XmlElement interface_name = AddElement (
                                        document, "InterfaceName", iface.FullName);
                                interface_node.AppendChild (interface_name);
                        }
                }

                //
                // <Attributes>
                //
                XmlElement class_attributes = document.CreateElement ("Attributes");
                object [] class_attrs = type.GetCustomAttributes (false);
                AddAttributes (document, class_attributes, class_attrs);

                type_node.AppendChild (class_attributes);

                //
                // <Members>
                //
                XmlElement members;

                //
                // delegates have an empty <Members> element.
                //
                if (isDelagate)
                        members = document.CreateElement ("Members");
                else
                        members = AddMembersNode (document, type);

                type_node.AppendChild (members);

                //
                // delegates have a top-level parameters and return value section
                //
                if (isDelagate) {
                        System.Reflection.MethodInfo method = type.GetMethod ("Invoke");
                        Type return_type = method.ReturnType;
                        ParameterInfo [] parameters = method.GetParameters ();
					
                        type_node.AppendChild (AddReturnValue (document, return_type));
                        type_node.AppendChild (AddParameters (document, parameters));
                }

                return document;
        }

        static string GetKeyString (byte [] key)
        {
                if (key.Length == 0)
                        return String.Empty;

                string s = BitConverter.ToString (key);
                s = s.Replace ('-', ' ');

                return '[' + s + ']';
        }

        static void AddAttributes (XmlDocument document, XmlElement root_element, object [] attributes)
        {
                if (attributes == null)
                        return;

                foreach (object attr in attributes) {
                        //
                        // Filter out the AssemblyFunkyAttributes
                        //
                        if (((Attribute) attr).GetType ().FullName.StartsWith ("System.Reflection.Assembly"))
                                continue;

                        else {
                                XmlElement attribute = document.CreateElement ("Attribute");
                                root_element.AppendChild (attribute);
                                XmlElement attribute_name = AddElement (document,
                                                "AttributeName", ((Attribute) attr).GetType ().FullName);
                                attribute.AppendChild (attribute_name);
                        }
                }
        }

        static XmlElement AddElement (XmlDocument document, string name, string text)
        {
                XmlElement element = document.CreateElement (name);

                if (text != null) {
                        XmlText text_node = document.CreateTextNode (text);
                        element.AppendChild (text_node);
                }

                return element;
        }

        static XmlElement AddDocsNode (XmlDocument document)
        {
                XmlElement docs = document.CreateElement ("Docs");
                docs.AppendChild (AddElement (document, "summary", EmptyString));
                docs.AppendChild (AddElement (document, "remarks", EmptyString));

                return docs;
        }

        static XmlElement AddDocsNode (XmlDocument document, Type return_value, ParameterInfo [] pi)
        {
                XmlElement docs = document.CreateElement ("Docs");
                docs.AppendChild (AddElement (document, "summary", EmptyString));

                if (pi != null)
                        foreach (ParameterInfo param in pi)
                                docs.AppendChild (AddDocsParamNode (document, param));

                XmlElement returns = AddDocsReturnsNode (document, return_value);

                if (returns != null)
                        docs.AppendChild (returns);

                docs.AppendChild (AddElement (document, "remarks", EmptyString));

                return docs;
        }

        static XmlElement AddDocsParamNode (XmlDocument document, ParameterInfo parameter)
        {
                Type param_type = parameter.ParameterType;
                XmlElement see_node = document.CreateElement ("see");
                see_node.SetAttribute ("cref", "T:" + param_type.GetElementType().ToString());

                XmlElement param = document.CreateElement ("param");
                param.SetAttribute ("name", parameter.Name);
                XmlText text_node =  document.CreateTextNode ("a ");
                param.AppendChild (text_node);
                param.AppendChild (see_node);

                return param;
        }

        static XmlElement AddDocsReturnsNode (XmlDocument document, Type return_value)
        {
                string return_type = ConvertCTSName (return_value.FullName);

                //
                // Return now if it returns void here.
                //
                if (return_type == "void")
                        return null;

                XmlElement see_node = document.CreateElement ("see");
                see_node.SetAttribute ("cref", "T:" + return_value.FullName);

                XmlElement param = document.CreateElement ("returns");
                XmlText text_node =  document.CreateTextNode ("a ");
                param.AppendChild (text_node);
                param.AppendChild (see_node);

                return param;
        }

        static XmlElement AddMembersNode (XmlDocument document, Type t)
        {
                XmlElement members = document.CreateElement ("Members");
                BindingFlags static_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Static;
                BindingFlags  instance_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Instance;

                foreach (FieldInfo fi in t.GetFields (static_flag))
                        AddField (members, fi);

                foreach (FieldInfo fi in t.GetFields (instance_flag))
                        AddField (members, fi);

                foreach (MethodInfo mi in t.GetMethods (static_flag))
                        AddMethod (members, mi);

                foreach (MethodInfo mi in t.GetMethods (instance_flag))
                        AddMethod (members, mi);

                foreach (ConstructorInfo ci in t.GetConstructors (static_flag))
                        AddConstructor (members, ci);

                foreach (ConstructorInfo ci in t.GetConstructors (instance_flag))
                        AddConstructor (members, ci);

                foreach (PropertyInfo pi in t.GetProperties (static_flag))
                        AddProperty (members, pi);

                foreach (PropertyInfo pi in t.GetProperties (instance_flag))
                        AddProperty (members, pi);

                foreach (EventInfo ei in t.GetEvents (static_flag))
                        AddEvent (members, ei);

                foreach (EventInfo ei in t.GetEvents (instance_flag))
                        AddEvent (members, ei);

                return members;
        }

        static void AddField (XmlElement members, FieldInfo field)
        {
                XmlDocument document = members.OwnerDocument;
                string signature = AddFieldSignature (field);

                if (signature == null)
                        return;

                XmlElement member = document.CreateElement ("Member");
                member.SetAttribute ("MemberName", field.Name);
                XmlElement field_signature = document.CreateElement ("MemberSignature");
                field_signature.SetAttribute ("Language", "C#");
                field_signature.SetAttribute ("Value", signature);

                members.AppendChild (member);
                member.AppendChild (field_signature);
                member.AppendChild (AddElement (document, "MemberType", "Field"));
                member.AppendChild (AddReturnValue (document, field.FieldType));
                member.AppendChild (AddElement (document, "Parameters", String.Empty));
                member.AppendChild (AddDocsNode (document));
        }

        static void AddMethod (XmlElement members, MethodInfo method)
        {
                XmlDocument document = members.OwnerDocument;
                string signature = AddMethodSignature (method);

                if (signature == null)
                        return;

                //
                // Filter out methods that are also properties
                //
                if (method.IsSpecialName)
                        return;

                //
                // Filter out methods from events
                // This is a lame hack, but there's no way around it.
                //
                if (method.Name.StartsWith ("add_") || method.Name.StartsWith ("remove_"))
                        return;

                XmlElement member = document.CreateElement ("Member");
                member.SetAttribute ("MemberName", method.Name);
                XmlElement method_signature = document.CreateElement ("MemberSignature");
                method_signature.SetAttribute ("Language", "C#");
                method_signature.SetAttribute ("Value", signature);

                members.AppendChild (member);
                member.AppendChild (method_signature);
                member.AppendChild (AddElement (document, "MemberType", "Method"));

                Type return_type = method.ReturnType;
                ParameterInfo [] parameters = method.GetParameters ();

                member.AppendChild (AddReturnValue (document, return_type));
                member.AppendChild (AddParameters (document, parameters));
                member.AppendChild (AddDocsNode (document, return_type, parameters));
        }

        static void AddConstructor (XmlElement members, ConstructorInfo constructor)
        {
                XmlDocument document = members.OwnerDocument;
                string signature = AddConstructorSignature (constructor);
                string constructor_name = constructor.Name;

                // .cctors are not suppose to be visible
                if (signature == null || constructor.Name == ".cctor")
                        return;

                XmlElement member = document.CreateElement ("Member");
                member.SetAttribute ("MemberName", constructor_name);
                members.AppendChild (member);
                XmlElement constructor_signature = document.CreateElement ("MemberSignature");
                constructor_signature.SetAttribute ("Language", "C#");
                constructor_signature.SetAttribute ("Value", signature);
                member.AppendChild (constructor_signature);
                member.AppendChild (AddElement (document, "MemberType", "Constructor"));

                Type return_type = constructor.DeclaringType;
                ParameterInfo [] parameters = constructor.GetParameters ();

                // constructors have an empty ReturnValue node.
                member.AppendChild (document.CreateElement ("ReturnValue"));
                member.AppendChild (AddParameters (document, parameters));
                member.AppendChild (AddDocsNode (document, return_type, parameters));
        }

        static void AddProperty (XmlElement members, PropertyInfo property)
        {
                XmlDocument document = members.OwnerDocument;
                string signature = AddPropertySignature (property);

                if (signature == null)
                        return;

                XmlElement member = document.CreateElement ("Member");
                member.SetAttribute ("MemberName", property.Name);
                members.AppendChild (member);
                XmlElement property_signature = document.CreateElement ("MemberSignature");
                property_signature.SetAttribute ("Language", "C#");
                property_signature.SetAttribute ("Value", signature);
                member.AppendChild (property_signature);
                member.AppendChild (AddElement (document, "MemberType", "Property"));

                Type return_type = property.PropertyType;
                member.AppendChild (AddReturnValue (document, return_type));

                if (property.CanRead && property.GetGetMethod () != null) {
                        ParameterInfo [] parameters = property.GetGetMethod ().GetParameters ();
                        member.AppendChild (AddParameters (document, parameters));
                        member.AppendChild (AddDocsNode (document, return_type, parameters));
                } else
                        member.AppendChild (AddDocsNode (document, return_type, null));
        }

        static void AddEvent (XmlElement members, EventInfo ev)
        {
                XmlDocument document = members.OwnerDocument;
                string signature = AddEventSignature (ev);

                if (signature == null)
                        return;

                XmlElement member = document.CreateElement ("Member");
                member.SetAttribute ("MemberName", ev.Name);
                members.AppendChild (member);
                XmlElement event_signature = document.CreateElement ("MemberSignature");
                event_signature.SetAttribute ("Language", "C#");
                event_signature.SetAttribute ("Value", signature);
                member.AppendChild (event_signature);
                member.AppendChild (AddElement (document, "MemberType", "Event"));
                member.AppendChild (AddReturnValue (document, ev.EventHandlerType));
                member.AppendChild (AddElement (document, "Parameters", null));
                member.AppendChild (AddDocsNode (document));
        }

        static XmlElement AddReturnValue (XmlDocument document, Type retval)
        {
                XmlElement return_value = document.CreateElement ("ReturnValue");
                XmlElement return_type = document.CreateElement ("ReturnType");
                XmlText value = document.CreateTextNode (retval.FullName);

                return_type.AppendChild (value);
                return_value.AppendChild (return_type);

                return return_value;
        }

        static XmlElement AddParameters (XmlDocument document, ParameterInfo [] pi)
        {
                XmlElement parameters = document.CreateElement ("Parameters");

                foreach (ParameterInfo p in pi) {
                        XmlElement parameter = document.CreateElement ("Parameter");
                        parameter.SetAttribute ("Name", p.Name);
                        parameter.SetAttribute ("Type", p.ParameterType.FullName);

                        if (p.ParameterType.IsByRef) {
                                if (!p.IsOut)
                                        parameter.SetAttribute ("RefType", "ref");
                                else
                                        parameter.SetAttribute ("RefType", "out");
                        }
                        parameters.AppendChild (parameter);
                }

                return parameters;
        }

        static string GetTypeKind (Type t)
        {
                if (t.IsEnum || t == typeof (System.Enum))
                        return "enum";
                if (t.IsClass)
                        return "class";
                if (t.IsInterface)
                        return "interface";
                if (t.IsValueType)
                        return "struct";
                else
                        throw new ArgumentException ();
        }

        static string GetTypeVisibility (Type t)
        {
                switch (t.Attributes & TypeAttributes.VisibilityMask){
                case TypeAttributes.Public:
			return "public";
                case TypeAttributes.NestedPublic:
                        return GetTypeVisibility (t.DeclaringType) != null ? "public" : null;

                case TypeAttributes.NestedFamily:
                case TypeAttributes.NestedFamANDAssem:
                case TypeAttributes.NestedFamORAssem:
                        return GetTypeVisibility (t.DeclaringType) != null ? "protected" : null;

                default:
                        return null;
                }
        }

        static string AddTypeSignature (Type type, out bool isDelagate)
        {
                // Assume it is not a delegate
                isDelagate = false;

                if (type == null)
                        return null;

                string signature;
                bool colon = true;

                string name = type.Name;
                StringBuilder extends = new StringBuilder ();
                string modifiers = String.Empty;

                TypeAttributes ta = type.Attributes;
                string kind = GetTypeKind (type);
                string visibility = GetTypeVisibility (type);

                if (visibility == null)
                        return null;

                //
                // Modifiers
                //
                if (type.IsAbstract)
                        modifiers = " abstract";
                if (type.IsSealed)
                        modifiers =  " sealed";

                //
                // handle delegates
                //
                if (kind == "class" && !type.IsAbstract &&
                    typeof (System.Delegate).IsAssignableFrom (type)) {
                        isDelagate = true;
                        return AddDelegateSignature (visibility, modifiers, name, type);
                }

                //
                // get BaseType
                //
                if (type != typeof (object) && kind == "class" && type.BaseType != typeof (object)) {
                        extends.Append (" : " + type.BaseType.FullName);
                        colon = false;
                }

                //
                // Implements interfaces...
                //
                Type [] interfaces = type.GetInterfaces ();

                if (interfaces.Length != 0) {
                        if (colon)
                                extends.Append (" : ");
                        else
                                extends.Append (", ");

                        for (int i = 0; i < interfaces.Length; i ++){
                                extends.Append (interfaces [i].Name);
                                if (i + 1 != interfaces.Length) extends.Append (", ");
                        }
                }

                //
                // Put it together
                //
                switch (kind){
                case "class":
                        signature = String.Format ("{0}{1} {2} {3}{4}",
                                        visibility, modifiers, kind, name, extends.ToString ());
                        break;

                case "enum":
                        signature = String.Format ("{0} {1} {2}",
                                        visibility, kind, name);
                        break;

                case "interface":
                case "struct":
                default:                        
                        signature = String.Format ("{0}{1} {2} {3}",
                                        visibility, modifiers, kind, name);
                        break;
                }

                return signature;
        }

        static string AddDelegateSignature (string visibility, string modifiers, string name, Type type)
        {
                string signature;

                MethodInfo invoke = type.GetMethod ("Invoke");
                string arguments = GetMethodParameters (invoke);
                string return_value = ConvertCTSName (invoke.ReturnType.FullName);

                signature = String.Format ("{0}{1} delegate {2} {3} {4};",
                                visibility, modifiers, return_value, name, arguments);

                return signature;
        }

        static string GetFieldVisibility (FieldInfo field)
        {
                if (field.IsPublic)
                        return "public";

                if (field.IsFamily)
                        return "protected";

                else
                        return null;
        }

        static string GetFieldModifiers (FieldInfo field)
        {
                if (field.IsStatic)
                        return " static";

                if (field.IsInitOnly)
                        return " readonly";

                else
                        return null;
        }

        static string GetMethodVisibility (MethodBase method)
        {
                if (method.IsPublic)
                        return "public";

                if (method.IsFamily)
                        return "protected";
                else
                        return null;
        }

        static string GetMethodModifiers (MethodBase method)
        {
                if (method.IsStatic)
                        return " static";

                if (method.IsVirtual) {
                        if ((method.Attributes & MethodAttributes.NewSlot) != 0)
                                return " virtual";
                        else
                                return " override";

                } else
                        return null;
        }

        static string GetMethodParameters (MethodBase method)
        {
                StringBuilder sb;
                ParameterInfo [] pi = method.GetParameters ();

                if (pi.Length == 0)
                        return "()";

                else {
                        sb = new StringBuilder ();
                        sb.Append ('(');

                        int i = 0;
                        string modifier;
                        foreach (ParameterInfo parameter in pi) {
                                bool isPointer = false;
                                if (parameter.ParameterType.IsByRef) {
                                        sb.Append (GetParameterModifier (parameter));
                                        isPointer = true;
                                }
                                string param = ConvertCTSName (parameter.ParameterType.FullName, isPointer);

                                sb.Append (param);
                                sb.Append (" " + parameter.Name);
                                if (i + 1 < pi.Length) sb.Append (", ");
                                i++;
                        }
                        sb.Append (')');
                }

                return sb.ToString ();
        }

        static string GetParameterModifier (ParameterInfo parameter)
        {
                int a = (int) parameter.Attributes;
                if (parameter.IsOut)
                        return "out ";

                return "ref ";
        }

        static string GetPropertyVisibility (PropertyInfo property)
        {
                MethodBase mb = property.GetSetMethod (true);

                if (mb == null)
                        mb = property.GetGetMethod (true);

                return GetMethodVisibility (mb);
        }

        static string GetPropertyModifiers (PropertyInfo property)
        {
                MethodBase mb = property.GetSetMethod (true);

                if (mb == null)
                        mb = property.GetGetMethod (true);

                return GetMethodModifiers (mb);
        }

        static string GetEventModifiers (EventInfo ev)
        {
                return GetMethodModifiers (ev.GetAddMethod ());
        }

        static string GetEventVisibility (EventInfo ev)
        {
                MethodInfo add = ev.GetAddMethod ();
                if (add == null)
                        return null;
                return GetMethodVisibility (add);
        }

        static string GetEventType (EventInfo ev)
        {
                ParameterInfo [] pi = ev.GetAddMethod ().GetParameters ();

                if (pi.Length != 1)
                        throw new ArgumentException ("There is more than one argument to the add_ method of this event.");

                return ConvertCTSName (pi [0].ParameterType.FullName);
        }

        static string AddFieldSignature (FieldInfo field)
        {
                string signature;
                string visibility = GetFieldVisibility (field);

                if (visibility == null)
                        return null;

                string type = ConvertCTSName (field.FieldType.FullName);
                string name = field.Name;
                string modifiers = GetFieldModifiers (field);

                signature = String.Format ("{0}{1} {2} {3};",
                                visibility, modifiers, type, name);

                if (field.DeclaringType.IsEnum)
                        signature = name;
                
                return signature;
        }

        static string AddMethodSignature (MethodInfo method)
        {
                string signature;
                string visibility = GetMethodVisibility (method);

                if (visibility == null)
                        return null;

                string modifiers = GetMethodModifiers (method);
                string return_type = ConvertCTSName (method.ReturnType.FullName);
                string method_name = method.Name;
                string parameters = GetMethodParameters (method);

                signature = String.Format ("{0}{1} {2} {3} {4};",
                                visibility, modifiers, return_type, method_name, parameters);

                return signature;
        }

        static string AddConstructorSignature (ConstructorInfo constructor)
        {
                string signature;
                string visibility = GetMethodVisibility (constructor);

                if (visibility == null)
                        return null;

                string modifiers = GetMethodModifiers (constructor);
                string name = constructor.DeclaringType.Name;
                string parameters = GetMethodParameters (constructor);

                signature = String.Format ("{0}{1} {2} {3};",
                                visibility, modifiers, name, parameters);

                return signature;
        }

        static string AddPropertySignature (PropertyInfo property)
        {
                string signature;
                string visibility = GetPropertyVisibility (property);

                if (visibility == null)
                        return null;

                string modifiers = GetPropertyModifiers (property);
                string name = property.Name;

                string type_name = property.PropertyType.FullName;

                if (property.PropertyType.IsArray) {
                        int i = type_name.IndexOf ('[');
                        if (type_name [i - 1] != ' ')
                                type_name = type_name.Insert (i, " "); // always put a space before the []
                }

                string return_type = ConvertCTSName (type_name);
                string arguments = null;

                if (property.CanRead && property.CanWrite)
                        arguments = "{ set; get; }";

                else if (property.CanRead)
                        arguments = "{ get; }";

                else if (property.CanWrite)
                        arguments = "{ set; }";

                signature = String.Format ("{0}{1} {2} {3} {4};",
                                visibility, modifiers, return_type, name, arguments);

                return signature;
        }

        static string AddEventSignature (EventInfo ev)
        {
                string signature;
                string visibility = GetEventVisibility (ev);

                if (visibility == null)
                        return null;

                string modifiers = GetEventModifiers (ev);
                string name = ev.Name;
                string type = GetEventType (ev);

                signature = String.Format ("{0}{1} event {2} {3};",
                                visibility, modifiers, type, name);

                return signature;
        }

        static string ConvertCTSName (string type, bool shorten)
        {
                if (shorten)
                        type =  type.Substring (0, type.Length - 1);

                string retval =  ConvertCTSName (type);

                return retval;
        }

        //
        // Utility function: converts a fully .NET qualified type name into a C#-looking one
        //
        static string ConvertCTSName (string type)
        {
                string retval = String.Empty;
                bool isArray = false;
                bool isPointer = false;

                if (!type.StartsWith ("System."))
                        return type;

                if (type.EndsWith ("[]")) {
                        isArray = true;
                        type = type.Substring (0, type.Length - 2);
                        type = type.TrimEnd ();
                }

                if (type.EndsWith ("&")) {
                        isPointer = true;
                        type = type.Substring (0, type.Length - 1);
                        type = type.TrimEnd ();
                }

                switch (type) {
                case "System.Byte": retval = "byte"; break;
                case "System.SByte": retval = "sbyte"; break;
                case "System.Int16": retval = "short"; break;
                case "System.Int32": retval = "int"; break;
                case "System.Int64": retval = "long"; break;

                case "System.UInt16": retval = "ushort"; break;
                case "System.UInt32": retval = "uint"; break;
                case "System.UInt64": retval = "ulong"; break;

                case "System.Single":  retval = "float"; break;
                case "System.Double":  retval = "double"; break;
                case "System.Decimal": retval = "decimal"; break;
                case "System.Boolean": retval = "bool"; break;
                case "System.Char":    retval = "char"; break;
                case "System.Void":    retval = "void"; break;
                case "System.String":  retval = "string"; break;
                case "System.Object":  retval = "object"; break;

                default:
                        if (type.StartsWith (Namespace))
                                retval = type.Substring (Namespace.Length + 1);
                        else if (type.StartsWith ("System") && 
                                        (type.IndexOf ('.') == type.LastIndexOf ('.')))
                                retval = type.Substring (7);
                        else
                                retval = type;
                        break;
                }

                if (isArray)
                        retval = retval + " []";

                if (isPointer)
                        retval = retval + "&";

                return retval;
        }
	
	static string GetName (Type t)
	{
		string s = "";
		while (t.DeclaringType != t) {
			s = "+" + t.Name;
			t = t.DeclaringType;
		}
		
		return t.Name + s;
	}
}
