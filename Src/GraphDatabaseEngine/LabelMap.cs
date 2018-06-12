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
        private DomainStore Store { get; }
        public List<OperatorInfo> OperatorList { get; }
        public Dictionary<String, List<string>> LabelFragmentsMap { get; }

        public class OperatorInfo
        {
            public bool isCountComparison;
            public bool isValueComparison;
            public RelKind Operator;
            public String Label;
            public Cnst Cnst;

            public OperatorInfo(RelKind op, string label, Cnst cnst, bool isCount)
            {
                Operator = op;
                Label = label;
                Cnst = cnst;

                if (isCount)
                {
                    isCountComparison = true;
                    isValueComparison = false;
                }
                else
                {
                    isCountComparison = false;
                    isValueComparison = true;
                }
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

        public LabelMap(Body body, DomainStore store)
        {
            LabelInfoMap = new Dictionary<string, List<LabelInfo>>();
            BindingMap = new Dictionary<string, LabelInfo>();
            OperatorList = new List<OperatorInfo>();
            LabelFragmentsMap = new Dictionary<string, List<string>>();

            Store = store;

            CreateLabelMap(body);
        }

        public bool IsBindingLabel(string label)
        {
            if (BindingMap.ContainsKey(label)) return true;
            else return false;
        }

        public bool HasFragment(string label)
        {
            if (!LabelFragmentsMap.ContainsKey(label))
            {
                return false;
            }

            List<string> list = LabelFragmentsMap[label];
            if (list.Count() > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Get all related labels like "a.b.c" related to label "a".
        public List<string> GetRelatedLabelsWithFragments(string label)
        {
            List<string> relatedLabels = new List<string>();
            if (!LabelFragmentsMap.ContainsKey(label))
            {
                return relatedLabels;
            }

            foreach (string key in LabelFragmentsMap.Keys)
            {
                var fragments = LabelFragmentsMap[key];
                if (fragments.Count() > 1 && fragments.ElementAt(0) == label)
                {
                    relatedLabels.Add(key);
                }
            }
            return relatedLabels;
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
            List<string> labels = LabelInfoMap.Keys.ToList();
            List<string> bindingLabels = BindingMap.Keys.ToList();
            List<string> allLabels = new List<string>();
            foreach (var label in labels)
            {
                allLabels.Add(label);
            }

            foreach (var label in bindingLabels)
            {
                allLabels.Add(label);
            }

            return allLabels;
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
            else
            {
                if (LabelInfoMap.ContainsKey(label))
                {
                    // Find the type of the label. Check if it is basic built-in type or other types.
                    List<LabelInfo> labelInfoList;
                    LabelInfoMap.TryGetValue(label, out labelInfoList);
                    string labelFuncType = labelInfoList[0].Type;
                    int labelIndex = labelInfoList[0].ArgIndex;
                    List<String> argTypeList = Store.GetArgTypes(labelFuncType);
                    string labelType = argTypeList[labelIndex];
                    return labelType;
                }
                else
                {
                    // Label is neither a binding label nor a label in one of the constructor.
                    List<string> fragments = LabelFragmentsMap[label];
                    string bindingLabel = fragments.ElementAt(0);
                    var bindingLabelInfo = BindingMap[bindingLabel];
                    string bindingLabelType = bindingLabelInfo.Type;
                    List<string> argLabelList = Store.TypeArgsLabelMap[bindingLabelType];
                    string argType = bindingLabelType;

                    for (int i=1; i<fragments.Count(); i++)
                    {
                        string argLabel = fragments[i];
                        int index = argLabelList.IndexOf(argLabel);
                        argType = Store.TypeArgsMap[argType].ElementAt(index);
                        if (Store.TypeArgsLabelMap.ContainsKey(argType))
                        {
                            argLabelList = Store.TypeArgsLabelMap[argType];
                        }
                    }

                    return argType;
                }
            }
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
            string idType = Store.GetModelType(id);
            List<string> argTypes = Store.GetArgTypes(idType);
            //TypeArgsMap.TryGetValue(idType, out argTypes);
            return argTypes[index];
        }

        public List<LabelInfo> GetLabelOccuranceInfo(string label)
        {
            if (!LabelInfoMap.ContainsKey(label))
            {
                return null;
            }
            List<LabelInfo> list;
            LabelInfoMap.TryGetValue(label, out list);
            return list;
        }

        public void CreateLabelMap(Body body)
        {
            Dictionary<String, int> typeCounts = new Dictionary<string, int>();
            CreateLabelMap(body, typeCounts);
        }

        public void CreateLabelMap(Body body, Dictionary<String, int> typeCounts)
        {        
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

                        // Add fragment list into map for labels like "a.b.property".
                        if (!LabelFragmentsMap.ContainsKey(label))
                        {
                            LabelFragmentsMap.Add(label, id.Fragments.ToList());
                        }

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
                        LabelFragmentsMap.Add(binding.Name, binding.Fragments.ToList());
                    }
                }
                else if (element.NodeKind == NodeKind.RelConstr)
                {
                    RelConstr relConstr = element as RelConstr;
                    Cnst value = relConstr.Arg2 as Cnst;
                    // Compare value like high != 2 in rules if label "high" represent a Integer.
                    if (relConstr.Arg1.NodeKind == NodeKind.Id)
                    {
                        OperatorInfo info = new OperatorInfo(relConstr.Op, (relConstr.Arg1 as Id).Name, value, false);
                        OperatorList.Add(info);
                        Id id = relConstr.Arg1 as Id;
                        if (!LabelFragmentsMap.ContainsKey(id.Name))
                        {
                            LabelFragmentsMap.Add(id.Name, id.Fragments.ToList());
                        }
                    }
                    else
                    {
                        Compr compr;
                        // count({s | s...}) > 0
                        if (relConstr.Arg1.NodeKind == NodeKind.FuncTerm)
                        {
                            FuncTerm ft = relConstr.Arg1 as FuncTerm;
                            compr = ft.Args.ElementAt(0) as Compr;
                        }
                        else // no {s | s...}
                        {
                            compr = relConstr.Arg1 as Compr;
                        }
                        
                        string label = (compr.Heads.ElementAt(0) as Id).Name;
                        Body comprBody = (compr.Bodies.ElementAt(0) as Body);
                        OperatorInfo info = new OperatorInfo(relConstr.Op, label, value, true);
                        OperatorList.Add(info);
                        // Recursively Add labels inside count({s | ...}) into label map.
                        CreateLabelMap(comprBody, typeCounts);
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

            // For label "cc", add all labels like "cc.x" into label list.
            foreach (string labelWithFragments in LabelFragmentsMap.Keys)
            {
                if (!labels.Contains(labelWithFragments) && labelWithFragments.Contains(label))
                {
                    labels.Add(labelWithFragments);
                    FindSCCLabels(labels, labelWithFragments);
                }
            }

            // For label cc.x, add "cc" into label list.
            if (HasFragment(label))
            {
                List<string> fragments = LabelFragmentsMap[label];
                if (!labels.Contains(fragments.ElementAt(0)))
                {
                    labels.Add(fragments.ElementAt(0));
                    FindSCCLabels(labels, fragments.ElementAt(0));
                }
            }

            // Check if the label is an argument label or binding label for instance.
            if (BindingMap.Keys.Contains(label))
            {
                // Find all argument labels belonged to a binding label like cc is C(b, c) and add them into label set.
                LabelInfo bindingLabelInfo = BindingMap[label];
                foreach (var key in LabelInfoMap.Keys)
                {
                    if (!labels.Contains(key))
                    {
                        var list = LabelInfoMap[key];
                        foreach (LabelInfo info in list)
                        {
                            // Add all labels related to the instance label into SCCLabel set.
                            if (info.Type == bindingLabelInfo.Type && info.InstanceIndex == bindingLabelInfo.InstanceIndex)
                            {
                                labels.Add(key);
                                FindSCCLabels(labels, key);
                            }
                        }
                    }   
                }

            }
            else
            {
                // For labels like cc.y = 2 that it does not occur in constructor.
                if (LabelInfoMap.ContainsKey(label))
                {
                    // Add other labels inside the same constructor.
                    List<LabelInfo> labelInfoList;
                    LabelInfoMap.TryGetValue(label, out labelInfoList);

                    // srcTuples represents all occurance of target label.
                    foreach (var labelInfo in labelInfoList)
                    {
                        string typeName = labelInfo.Type;
                        int count = labelInfo.InstanceIndex;
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

                        // Don't forget to check binding label map and add binding label.
                        foreach (string bindingLabel in BindingMap.Keys)
                        {
                            if (!labels.Contains(bindingLabel))
                            {
                                LabelInfo bindingLabelInfo = BindingMap[bindingLabel];
                                if (labelInfo.Type == bindingLabelInfo.Type && labelInfo.InstanceIndex == bindingLabelInfo.InstanceIndex)
                                {
                                    labels.Add(bindingLabel);
                                    FindSCCLabels(labels, bindingLabel);
                                }
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
