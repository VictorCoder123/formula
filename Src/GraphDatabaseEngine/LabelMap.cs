namespace Microsoft.Formula.GraphDatabaseEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Formula.API;
    using Microsoft.Formula.API.ASTQueries;
    using Microsoft.Formula.API.Nodes;
    using Microsoft.Formula.Common;
    using Microsoft.Formula.API.Generators;

    public class LabelMap
    {
        private Dictionary<String, List<LabelInfo>> LabelInfoMap { get; }
        private Dictionary<String, LabelInfo> BindingMap { get; }
        public List<OperatorInfo> OperatorList { get; }

        // Map Id of ModelFact to FuncTerm with original Id or auto-generated Id.
        public Dictionary<String, String> IdTypeMap { get; }
        // Map type name to a list of type in arguments.
        public Dictionary<String, List<String>> TypeArgsMap { get; }

        public class OperatorInfo
        {
            public RelKind Operator;
            public String Label;
            public Cnst Cnst;

            public OperatorInfo(RelKind op, string label, Cnst cnst)
            {
                Operator = op;
                Label = label;
                Cnst = cnst;
            }
        }

        public class LabelInfo
        {
            // label is binded either to instance or argument.
            public bool isBindingLabel { get; }
            public string Type { get; }
            public int ArgIndex { get; }
            public int InstanceIndex { get; }

            public LabelInfo(string type, int argIndex, int instanceIndex)
            {
                Type = type;
                ArgIndex = argIndex;
                InstanceIndex = instanceIndex;
                isBindingLabel = false;
            }

            public LabelInfo(string type, int instanceIndex)
            {
                Type = type;
                InstanceIndex = instanceIndex;
                isBindingLabel = true;
            }
        }

        public LabelMap(Body body, Dictionary<String, String> idTypeMap, Dictionary<String, List<String>> typeArgsMap)
        {
            LabelInfoMap = new Dictionary<string, List<LabelInfo>>();
            BindingMap = new Dictionary<string, LabelInfo>();
            OperatorList = new List<OperatorInfo>();

            IdTypeMap = idTypeMap;
            TypeArgsMap = typeArgsMap;

            CreateLabelMap(body);
        }

        // Get binding label given type and the index of instance.
        public string GetBindingLabel(string type, int instanceIndex)
        {
            foreach (string label in BindingMap.Keys)
            {
                LabelInfo info;
                BindingMap.TryGetValue(label, out info);
                if (info.Type == type && info.InstanceIndex == instanceIndex)
                {
                    return label;
                }
            }
            return null;
        }

        public List<string> GetAllLabels()
        {
            return LabelInfoMap.Keys.ToList();
        }

        // Infer the type of label in FORMULA rule from label map.
        public string GetLabelType(string label)
        {
            if (BindingMap.Keys.Contains(label))
            {
                LabelInfo info;
                BindingMap.TryGetValue(label, out info);
                return info.Type;
            }
            // Find the type of the label. Check if it is basic built-in type or other types.
            List<LabelInfo> labelInfoList;
            LabelInfoMap.TryGetValue(label, out labelInfoList);
            string labelFuncType = labelInfoList[0].Type;
            int labelIndex = labelInfoList[0].ArgIndex;
            List<String> argTypeList;
            TypeArgsMap.TryGetValue(labelFuncType, out argTypeList);
            string labelType = argTypeList[labelIndex];
            return labelType;
        }

        public int GetLabelIndex(string label)
        {
            // Find the type of the label. Check if it is basic built-in type or other types.
            List<LabelInfo> labelInfoList;
            LabelInfoMap.TryGetValue(label, out labelInfoList);
            int labelIndex = labelInfoList[0].ArgIndex;
            return labelIndex;
        }

        // Get the type of argument corresponding to unique Id given Id and index.
        public string GetArgType(string id, int index)
        {
            string idType;
            IdTypeMap.TryGetValue(id, out idType);
            List<string> argTypes;
            TypeArgsMap.TryGetValue(idType, out argTypes);
            return argTypes[index];
        }

        public List<LabelInfo> GetLabelOccuranceInfo(string label)
        {
            List<LabelInfo> list;
            LabelInfoMap.TryGetValue(label, out list);
            return list;
        }

        public void CreateLabelMap(Body body)
        {        
            Dictionary<String, int> typeCounts = new Dictionary<string, int>();

            foreach (var element in body.Children)
            {
                if (element.NodeKind == NodeKind.Find)
                {
                    FuncTerm ft = ((Find)element).Match as FuncTerm;
                    Id binding = ((Find)element).Binding;
                    string typeName = ((Id)ft.Function).Name;

                    // Count the occurance of same type name in query.
                    if (!typeCounts.ContainsKey(typeName))
                    {
                        typeCounts.Add(typeName, 0);
                    }
                    else
                    {
                        int oldCount;
                        typeCounts.TryGetValue(typeName, out oldCount);
                        typeCounts[typeName] = oldCount + 1;
                    }

                    int currentCount;
                    typeCounts.TryGetValue(typeName, out currentCount);

                    for (int i = 0; i < ft.Args.Count(); i++)
                    {
                        Id id = (Id)ft.Args.ElementAt(i);
                        string label = id.Name;
                        if (!LabelInfoMap.ContainsKey(label))
                        {
                            List<LabelInfo> list = new List<LabelInfo>();
                            LabelInfoMap.Add(label, list);
                        }
                        List<LabelInfo> labelInfoList;
                        LabelInfoMap.TryGetValue(label, out labelInfoList);
                        LabelInfo info = new LabelInfo(typeName, i, currentCount);
                        labelInfoList.Add(info);
                    }

                    if (binding != null)
                    {
                        LabelInfo info = new LabelInfo(typeName, currentCount);
                        BindingMap.Add(binding.Name, info);
                    }
                }
                else if (element.NodeKind == NodeKind.RelConstr)
                {
                    RelConstr relConstr = element as RelConstr;
                    Cnst value = relConstr.Arg2 as Cnst;
                    if (relConstr.Arg1.NodeKind == NodeKind.Id)
                    {
                        OperatorInfo info = new OperatorInfo(relConstr.Op, (relConstr.Arg1 as Id).Name, value);
                        OperatorList.Add(info);
                    }
                    else if (relConstr.Arg1.NodeKind == NodeKind.FuncTerm)
                    {
                        FuncTerm ft = relConstr.Arg1 as FuncTerm;
                        Compr compr = ft.Args.ElementAt(0) as Compr;
                        string label = (compr.Heads.ElementAt(0) as Id).Name;
                        Body comprBody = (compr.Bodies.ElementAt(0) as Body);
                        OperatorInfo info = new OperatorInfo(relConstr.Op, label, value);
                        OperatorList.Add(info);
                        // Recursively Add labels inside count({s | ...}) into label map.
                        CreateLabelMap(comprBody);
                    }
                }               
            }
        }

        // Overload and return a list of labels that are all strongly connected to a given label including itself.
        public HashSet<String> FindSCCLabels(string label)
        {
            HashSet<String> relatedLabels = new HashSet<string>();
            FindSCCLabels(relatedLabels, label);
            return relatedLabels;
        }

        // Recursively return a list of labels that are all strongly connected.
        public HashSet<String> FindSCCLabels(HashSet<String> labels, string label)
        {
            labels.Add(label);
            List<LabelInfo> labelInfoList;
            LabelInfoMap.TryGetValue(label, out labelInfoList);

            // srcTuples represents all occurance of target label.
            foreach (var labelInfo in labelInfoList)
            {
                string typeName = labelInfo.Type;
                int count = labelInfo.InstanceIndex;

                // Traverse all labels to find matches for target label.
                foreach (var dstLabel in LabelInfoMap.Keys)
                {
                    if (!labels.Contains(dstLabel))
                    {
                        List<LabelInfo> dstTuples;
                        LabelInfoMap.TryGetValue(dstLabel, out dstTuples);
                        foreach (var dstTuple in dstTuples)
                        {
                            if (dstTuple.Type == typeName && dstTuple.InstanceIndex == count)
                            {
                                labels.Add(dstLabel);
                                FindSCCLabels(labels, dstLabel);
                            }
                        }
                    }
                }
            }

            return labels;
        }

        // Find a list of strongly connected components from all labels.
        public List<HashSet<string>> GetSCCGroups(List<string> labels)
        {
            List<HashSet<string>> list = new List<HashSet<string>>();
            foreach (string label in labels)
            {
                bool exists = false;
                // Check if current label exists in one of the hashset.
                foreach (HashSet<string> set in list)
                {
                    if (set.Contains(label)) exists = true;
                }
                if (!exists)
                {
                    HashSet<string> scc = FindSCCLabels(label);
                    list.Add(scc);
                }
            }

            return list;
        }
    }
}
