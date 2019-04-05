namespace Microsoft.Formula.GraphDatabaseEngine.Domains
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics.Contracts;
   using System.Numerics;
   using System.Threading;
   using System.Threading.Tasks;
   using Microsoft.Formula.API;
   using Microsoft.Formula.API.Nodes;
   using Microsoft.Formula.API.Generators;
   using Microsoft.Formula.Common;
   using Microsoft.Formula.Common.Terms;

   public static partial class Formula_Root
   {
      private static readonly Dictionary<string, Func<ICSharpTerm[], ICSharpTerm>> ConstructorMap = new Dictionary<string, Func<ICSharpTerm[], ICSharpTerm>>();
      static Formula_Root()
      {
         ConstructorMap.Add("#Argument", args => MkUserCnst(Formula_Root.TypeCnstKind.Argument));
         ConstructorMap.Add("#Argument[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.Argument_NDEX_0));
         ConstructorMap.Add("#Argument[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.Argument_NDEX_1));
         ConstructorMap.Add("#Arguments", args => MkUserCnst(Formula_Root.TypeCnstKind.Arguments));
         ConstructorMap.Add("#Arguments[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.Arguments_NDEX_0));
         ConstructorMap.Add("#Arguments[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.Arguments_NDEX_1));
         ConstructorMap.Add("#Arguments[2]", args => MkUserCnst(Formula_Root.TypeCnstKind.Arguments_NDEX_2));
         ConstructorMap.Add("#BaseType", args => MkUserCnst(Formula_Root.TypeCnstKind.BaseType));
         ConstructorMap.Add("#BaseType[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.BaseType_NDEX_0));
         ConstructorMap.Add("#BaseType[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.BaseType_NDEX_1));
         ConstructorMap.Add("#BinaryExpr1", args => MkUserCnst(Formula_Root.TypeCnstKind.BinaryExpr1));
         ConstructorMap.Add("#BinaryExpr1[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.BinaryExpr1_NDEX_0));
         ConstructorMap.Add("#BinaryExpr1[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.BinaryExpr1_NDEX_1));
         ConstructorMap.Add("#BinaryExpr1[2]", args => MkUserCnst(Formula_Root.TypeCnstKind.BinaryExpr1_NDEX_2));
         ConstructorMap.Add("#BinaryExpr2", args => MkUserCnst(Formula_Root.TypeCnstKind.BinaryExpr2));
         ConstructorMap.Add("#BinaryExpr2[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.BinaryExpr2_NDEX_0));
         ConstructorMap.Add("#BinaryExpr2[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.BinaryExpr2_NDEX_1));
         ConstructorMap.Add("#BinaryExpr2[2]", args => MkUserCnst(Formula_Root.TypeCnstKind.BinaryExpr2_NDEX_2));
         ConstructorMap.Add("#BinaryExpr3", args => MkUserCnst(Formula_Root.TypeCnstKind.BinaryExpr3));
         ConstructorMap.Add("#BinaryExpr3[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.BinaryExpr3_NDEX_0));
         ConstructorMap.Add("#BinaryExpr3[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.BinaryExpr3_NDEX_1));
         ConstructorMap.Add("#BinaryExpr3[2]", args => MkUserCnst(Formula_Root.TypeCnstKind.BinaryExpr3_NDEX_2));
         ConstructorMap.Add("#Body", args => MkUserCnst(Formula_Root.TypeCnstKind.Body));
         ConstructorMap.Add("#BodySubterm", args => MkUserCnst(Formula_Root.TypeCnstKind.BodySubterm));
         ConstructorMap.Add("#BodySubterm[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.BodySubterm_NDEX_0));
         ConstructorMap.Add("#BodySubterm[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.BodySubterm_NDEX_1));
         ConstructorMap.Add("#Body[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.Body_NDEX_0));
         ConstructorMap.Add("#Body[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.Body_NDEX_1));
         ConstructorMap.Add("#Body[2]", args => MkUserCnst(Formula_Root.TypeCnstKind.Body_NDEX_2));
         ConstructorMap.Add("#BoolExpr", args => MkUserCnst(Formula_Root.TypeCnstKind.BoolExpr));
         ConstructorMap.Add("#BoolExpr[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.BoolExpr_NDEX_0));
         ConstructorMap.Add("#Boolean", args => MkUserCnst(Formula_Root.TypeCnstKind.Boolean));
         ConstructorMap.Add("#Count", args => MkUserCnst(Formula_Root.TypeCnstKind.Count));
         ConstructorMap.Add("#Count[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.Count_NDEX_0));
         ConstructorMap.Add("#Count[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.Count_NDEX_1));
         ConstructorMap.Add("#Domain", args => MkUserCnst(Formula_Root.TypeCnstKind.Domain));
         ConstructorMap.Add("#Domain[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.Domain_NDEX_0));
         ConstructorMap.Add("#EnumList", args => MkUserCnst(Formula_Root.TypeCnstKind.EnumList));
         ConstructorMap.Add("#EnumListSubterm", args => MkUserCnst(Formula_Root.TypeCnstKind.EnumListSubterm));
         ConstructorMap.Add("#EnumListSubterm[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.EnumListSubterm_NDEX_0));
         ConstructorMap.Add("#EnumListSubterm[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.EnumListSubterm_NDEX_1));
         ConstructorMap.Add("#EnumList[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.EnumList_NDEX_0));
         ConstructorMap.Add("#EnumList[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.EnumList_NDEX_1));
         ConstructorMap.Add("#EnumType", args => MkUserCnst(Formula_Root.TypeCnstKind.EnumType));
         ConstructorMap.Add("#EnumType[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.EnumType_NDEX_0));
         ConstructorMap.Add("#EnumType[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.EnumType_NDEX_1));
         ConstructorMap.Add("#Expr", args => MkUserCnst(Formula_Root.TypeCnstKind.Expr));
         ConstructorMap.Add("#FormulaBuiltInType", args => MkUserCnst(Formula_Root.TypeCnstKind.FormulaBuiltInType));
         ConstructorMap.Add("#Head", args => MkUserCnst(Formula_Root.TypeCnstKind.Head));
         ConstructorMap.Add("#Head[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.Head_NDEX_0));
         ConstructorMap.Add("#Head[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.Head_NDEX_1));
         ConstructorMap.Add("#Head[2]", args => MkUserCnst(Formula_Root.TypeCnstKind.Head_NDEX_2));
         ConstructorMap.Add("#Integer", args => MkUserCnst(Formula_Root.TypeCnstKind.Integer));
         ConstructorMap.Add("#InterpretedFunc", args => MkUserCnst(Formula_Root.TypeCnstKind.InterpretedFunc));
         ConstructorMap.Add("#Label", args => MkUserCnst(Formula_Root.TypeCnstKind.Label));
         ConstructorMap.Add("#Label[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.Label_NDEX_0));
         ConstructorMap.Add("#Label[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.Label_NDEX_1));
         ConstructorMap.Add("#Label[2]", args => MkUserCnst(Formula_Root.TypeCnstKind.Label_NDEX_2));
         ConstructorMap.Add("#Labels", args => MkUserCnst(Formula_Root.TypeCnstKind.Labels));
         ConstructorMap.Add("#LabelsSubterm", args => MkUserCnst(Formula_Root.TypeCnstKind.LabelsSubterm));
         ConstructorMap.Add("#LabelsSubterm[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.LabelsSubterm_NDEX_0));
         ConstructorMap.Add("#LabelsSubterm[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.LabelsSubterm_NDEX_1));
         ConstructorMap.Add("#Labels[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.Labels_NDEX_0));
         ConstructorMap.Add("#Labels[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.Labels_NDEX_1));
         ConstructorMap.Add("#Labels[2]", args => MkUserCnst(Formula_Root.TypeCnstKind.Labels_NDEX_2));
         ConstructorMap.Add("#Max", args => MkUserCnst(Formula_Root.TypeCnstKind.Max));
         ConstructorMap.Add("#Max[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.Max_NDEX_0));
         ConstructorMap.Add("#Max[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.Max_NDEX_1));
         ConstructorMap.Add("#Min", args => MkUserCnst(Formula_Root.TypeCnstKind.Min));
         ConstructorMap.Add("#Min[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.Min_NDEX_0));
         ConstructorMap.Add("#Min[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.Min_NDEX_1));
         ConstructorMap.Add("#ModelExpr", args => MkUserCnst(Formula_Root.TypeCnstKind.ModelExpr));
         ConstructorMap.Add("#ModelExpr[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.ModelExpr_NDEX_0));
         ConstructorMap.Add("#ModelExpr[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.ModelExpr_NDEX_1));
         ConstructorMap.Add("#Natural", args => MkUserCnst(Formula_Root.TypeCnstKind.Natural));
         ConstructorMap.Add("#NegInteger", args => MkUserCnst(Formula_Root.TypeCnstKind.NegInteger));
         ConstructorMap.Add("#Operator", args => MkUserCnst(Formula_Root.TypeCnstKind.Operator));
         ConstructorMap.Add("#PosInteger", args => MkUserCnst(Formula_Root.TypeCnstKind.PosInteger));
         ConstructorMap.Add("#Real", args => MkUserCnst(Formula_Root.TypeCnstKind.Real));
         ConstructorMap.Add("#Rule", args => MkUserCnst(Formula_Root.TypeCnstKind.Rule));
         ConstructorMap.Add("#Rule[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.Rule_NDEX_0));
         ConstructorMap.Add("#Rule[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.Rule_NDEX_1));
         ConstructorMap.Add("#Rule[2]", args => MkUserCnst(Formula_Root.TypeCnstKind.Rule_NDEX_2));
         ConstructorMap.Add("#String", args => MkUserCnst(Formula_Root.TypeCnstKind.String));
         ConstructorMap.Add("#Type", args => MkUserCnst(Formula_Root.TypeCnstKind.Type));
         ConstructorMap.Add("#UnionList", args => MkUserCnst(Formula_Root.TypeCnstKind.UnionList));
         ConstructorMap.Add("#UnionListSubterm", args => MkUserCnst(Formula_Root.TypeCnstKind.UnionListSubterm));
         ConstructorMap.Add("#UnionListSubterm[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.UnionListSubterm_NDEX_0));
         ConstructorMap.Add("#UnionListSubterm[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.UnionListSubterm_NDEX_1));
         ConstructorMap.Add("#UnionList[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.UnionList_NDEX_0));
         ConstructorMap.Add("#UnionList[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.UnionList_NDEX_1));
         ConstructorMap.Add("#UnionType", args => MkUserCnst(Formula_Root.TypeCnstKind.UnionType));
         ConstructorMap.Add("#UnionType[0]", args => MkUserCnst(Formula_Root.TypeCnstKind.UnionType_NDEX_0));
         ConstructorMap.Add("#UnionType[1]", args => MkUserCnst(Formula_Root.TypeCnstKind.UnionType_NDEX_1));
         ConstructorMap.Add("Argument", args => Formula_Root.MkArgument((Formula_Root.IArgType_Argument__0)args[0], (Formula_Root.IArgType_Argument__1)args[1]));
         ConstructorMap.Add("Arguments", args => Formula_Root.MkArguments((Formula_Root.IArgType_Arguments__0)args[0], (Formula_Root.IArgType_Arguments__1)args[1], (Formula_Root.IArgType_Arguments__2)args[2]));
         ConstructorMap.Add("BaseType", args => Formula_Root.MkBaseType((Formula_Root.IArgType_BaseType__0)args[0], (Formula_Root.IArgType_BaseType__1)args[1]));
         ConstructorMap.Add("BinaryExpr1", args => Formula_Root.MkBinaryExpr1((Formula_Root.IArgType_BinaryExpr1__0)args[0], (Formula_Root.IArgType_BinaryExpr1__1)args[1], (Formula_Root.IArgType_BinaryExpr1__2)args[2]));
         ConstructorMap.Add("BinaryExpr2", args => Formula_Root.MkBinaryExpr2((Formula_Root.IArgType_BinaryExpr2__0)args[0], (Formula_Root.IArgType_BinaryExpr2__1)args[1], (Formula_Root.IArgType_BinaryExpr2__2)args[2]));
         ConstructorMap.Add("BinaryExpr3", args => Formula_Root.MkBinaryExpr3((Formula_Root.IArgType_BinaryExpr3__0)args[0], (Formula_Root.IArgType_BinaryExpr3__1)args[1], (Formula_Root.IArgType_BinaryExpr3__2)args[2]));
         ConstructorMap.Add("Body", args => Formula_Root.MkBody((Formula_Root.IArgType_Body__0)args[0], (Formula_Root.IArgType_Body__1)args[1], (Formula_Root.IArgType_Body__2)args[2]));
         ConstructorMap.Add("BoolExpr", args => Formula_Root.MkBoolExpr((Formula_Root.IArgType_BoolExpr__0)args[0]));
         ConstructorMap.Add("Count", args => Formula_Root.MkCount((Formula_Root.IArgType_Count__0)args[0], (Formula_Root.IArgType_Count__1)args[1]));
         ConstructorMap.Add("Domain", args => Formula_Root.MkDomain((Formula_Root.IArgType_Domain__0)args[0]));
         ConstructorMap.Add("EQ", args => MkUserCnst(Formula_Root.UserCnstKind.EQ));
         ConstructorMap.Add("EnumList", args => Formula_Root.MkEnumList((Formula_Root.IArgType_EnumList__0)args[0], (Formula_Root.IArgType_EnumList__1)args[1]));
         ConstructorMap.Add("EnumType", args => Formula_Root.MkEnumType((Formula_Root.IArgType_EnumType__0)args[0], (Formula_Root.IArgType_EnumType__1)args[1]));
         ConstructorMap.Add("FALSE", args => MkUserCnst(Formula_Root.UserCnstKind.FALSE));
         ConstructorMap.Add("GT", args => MkUserCnst(Formula_Root.UserCnstKind.GT));
         ConstructorMap.Add("GTE", args => MkUserCnst(Formula_Root.UserCnstKind.GTE));
         ConstructorMap.Add("Head", args => Formula_Root.MkHead((Formula_Root.IArgType_Head__0)args[0], (Formula_Root.IArgType_Head__1)args[1], (Formula_Root.IArgType_Head__2)args[2]));
         ConstructorMap.Add("LT", args => MkUserCnst(Formula_Root.UserCnstKind.LT));
         ConstructorMap.Add("LTE", args => MkUserCnst(Formula_Root.UserCnstKind.LTE));
         ConstructorMap.Add("Label", args => Formula_Root.MkLabel((Formula_Root.IArgType_Label__0)args[0], (Formula_Root.IArgType_Label__1)args[1], (Formula_Root.IArgType_Label__2)args[2]));
         ConstructorMap.Add("Labels", args => Formula_Root.MkLabels((Formula_Root.IArgType_Labels__0)args[0], (Formula_Root.IArgType_Labels__1)args[1], (Formula_Root.IArgType_Labels__2)args[2]));
         ConstructorMap.Add("Max", args => Formula_Root.MkMax((Formula_Root.IArgType_Max__0)args[0], (Formula_Root.IArgType_Max__1)args[1]));
         ConstructorMap.Add("Min", args => Formula_Root.MkMin((Formula_Root.IArgType_Min__0)args[0], (Formula_Root.IArgType_Min__1)args[1]));
         ConstructorMap.Add("ModelExpr", args => Formula_Root.MkModelExpr((Formula_Root.IArgType_ModelExpr__0)args[0], (Formula_Root.IArgType_ModelExpr__1)args[1]));
         ConstructorMap.Add("NEQ", args => MkUserCnst(Formula_Root.UserCnstKind.NEQ));
         ConstructorMap.Add("NIL", args => MkUserCnst(Formula_Root.UserCnstKind.NIL));
         ConstructorMap.Add("Rule", args => Formula_Root.MkRule((Formula_Root.IArgType_Rule__0)args[0], (Formula_Root.IArgType_Rule__1)args[1], (Formula_Root.IArgType_Rule__2)args[2]));
         ConstructorMap.Add("TRUE", args => MkUserCnst(Formula_Root.UserCnstKind.TRUE));
         ConstructorMap.Add("UnionList", args => Formula_Root.MkUnionList((Formula_Root.IArgType_UnionList__0)args[0], (Formula_Root.IArgType_UnionList__1)args[1]));
         ConstructorMap.Add("UnionType", args => Formula_Root.MkUnionType((Formula_Root.IArgType_UnionType__0)args[0], (Formula_Root.IArgType_UnionType__1)args[1]));
         ConstructorMap.Add("Formula.#Any", args => MkUserCnst(Formula_Root.Formula.TypeCnstKind.Any));
         ConstructorMap.Add("Formula.#Constant", args => MkUserCnst(Formula_Root.Formula.TypeCnstKind.Constant));
         ConstructorMap.Add("Formula.#Data", args => MkUserCnst(Formula_Root.Formula.TypeCnstKind.Data));
      }

      public enum UserCnstKind
      {
         EQ,
         FALSE,
         GT,
         GTE,
         LT,
         LTE,
         NEQ,
         NIL,
         TRUE
      }

      public enum TypeCnstKind
      {
         Argument,
         Argument_NDEX_0,
         Argument_NDEX_1,
         Arguments,
         Arguments_NDEX_0,
         Arguments_NDEX_1,
         Arguments_NDEX_2,
         BaseType,
         BaseType_NDEX_0,
         BaseType_NDEX_1,
         BinaryExpr1,
         BinaryExpr1_NDEX_0,
         BinaryExpr1_NDEX_1,
         BinaryExpr1_NDEX_2,
         BinaryExpr2,
         BinaryExpr2_NDEX_0,
         BinaryExpr2_NDEX_1,
         BinaryExpr2_NDEX_2,
         BinaryExpr3,
         BinaryExpr3_NDEX_0,
         BinaryExpr3_NDEX_1,
         BinaryExpr3_NDEX_2,
         Body,
         BodySubterm,
         BodySubterm_NDEX_0,
         BodySubterm_NDEX_1,
         Body_NDEX_0,
         Body_NDEX_1,
         Body_NDEX_2,
         BoolExpr,
         BoolExpr_NDEX_0,
         Boolean,
         Count,
         Count_NDEX_0,
         Count_NDEX_1,
         Domain,
         Domain_NDEX_0,
         EnumList,
         EnumListSubterm,
         EnumListSubterm_NDEX_0,
         EnumListSubterm_NDEX_1,
         EnumList_NDEX_0,
         EnumList_NDEX_1,
         EnumType,
         EnumType_NDEX_0,
         EnumType_NDEX_1,
         Expr,
         FormulaBuiltInType,
         Head,
         Head_NDEX_0,
         Head_NDEX_1,
         Head_NDEX_2,
         Integer,
         InterpretedFunc,
         Label,
         Label_NDEX_0,
         Label_NDEX_1,
         Label_NDEX_2,
         Labels,
         LabelsSubterm,
         LabelsSubterm_NDEX_0,
         LabelsSubterm_NDEX_1,
         Labels_NDEX_0,
         Labels_NDEX_1,
         Labels_NDEX_2,
         Max,
         Max_NDEX_0,
         Max_NDEX_1,
         Min,
         Min_NDEX_0,
         Min_NDEX_1,
         ModelExpr,
         ModelExpr_NDEX_0,
         ModelExpr_NDEX_1,
         Natural,
         NegInteger,
         Operator,
         PosInteger,
         Real,
         Rule,
         Rule_NDEX_0,
         Rule_NDEX_1,
         Rule_NDEX_2,
         String,
         Type,
         UnionList,
         UnionListSubterm,
         UnionListSubterm_NDEX_0,
         UnionListSubterm_NDEX_1,
         UnionList_NDEX_0,
         UnionList_NDEX_1,
         UnionType,
         UnionType_NDEX_0,
         UnionType_NDEX_1
      }

      public static readonly string[] UserCnstNames =
      {
         "EQ",
         "FALSE",
         "GT",
         "GTE",
         "LT",
         "LTE",
         "NEQ",
         "NIL",
         "TRUE"
      };

      public static readonly string[] TypeCnstNames =
      {
         "#Argument",
         "#Argument[0]",
         "#Argument[1]",
         "#Arguments",
         "#Arguments[0]",
         "#Arguments[1]",
         "#Arguments[2]",
         "#BaseType",
         "#BaseType[0]",
         "#BaseType[1]",
         "#BinaryExpr1",
         "#BinaryExpr1[0]",
         "#BinaryExpr1[1]",
         "#BinaryExpr1[2]",
         "#BinaryExpr2",
         "#BinaryExpr2[0]",
         "#BinaryExpr2[1]",
         "#BinaryExpr2[2]",
         "#BinaryExpr3",
         "#BinaryExpr3[0]",
         "#BinaryExpr3[1]",
         "#BinaryExpr3[2]",
         "#Body",
         "#BodySubterm",
         "#BodySubterm[0]",
         "#BodySubterm[1]",
         "#Body[0]",
         "#Body[1]",
         "#Body[2]",
         "#BoolExpr",
         "#BoolExpr[0]",
         "#Boolean",
         "#Count",
         "#Count[0]",
         "#Count[1]",
         "#Domain",
         "#Domain[0]",
         "#EnumList",
         "#EnumListSubterm",
         "#EnumListSubterm[0]",
         "#EnumListSubterm[1]",
         "#EnumList[0]",
         "#EnumList[1]",
         "#EnumType",
         "#EnumType[0]",
         "#EnumType[1]",
         "#Expr",
         "#FormulaBuiltInType",
         "#Head",
         "#Head[0]",
         "#Head[1]",
         "#Head[2]",
         "#Integer",
         "#InterpretedFunc",
         "#Label",
         "#Label[0]",
         "#Label[1]",
         "#Label[2]",
         "#Labels",
         "#LabelsSubterm",
         "#LabelsSubterm[0]",
         "#LabelsSubterm[1]",
         "#Labels[0]",
         "#Labels[1]",
         "#Labels[2]",
         "#Max",
         "#Max[0]",
         "#Max[1]",
         "#Min",
         "#Min[0]",
         "#Min[1]",
         "#ModelExpr",
         "#ModelExpr[0]",
         "#ModelExpr[1]",
         "#Natural",
         "#NegInteger",
         "#Operator",
         "#PosInteger",
         "#Real",
         "#Rule",
         "#Rule[0]",
         "#Rule[1]",
         "#Rule[2]",
         "#String",
         "#Type",
         "#UnionList",
         "#UnionListSubterm",
         "#UnionListSubterm[0]",
         "#UnionListSubterm[1]",
         "#UnionList[0]",
         "#UnionList[1]",
         "#UnionType",
         "#UnionType[0]",
         "#UnionType[1]"
      };

      public static string Namespace { get { return ""; } }

      public static bool CreateObjectGraph(Env env, ProgramName progName, string modelName, out Task<ObjectGraphResult> task)
      {
         Contract.Requires(env != null && progName != null && !string.IsNullOrEmpty(modelName));
         return env.CreateObjectGraph(progName, modelName, MkNumeric, MkString, ConstructorMap, out task);
      }

      public static RealCnst MkNumeric(int val)
      {
         var n = new RealCnst();
         n.Value = new Rational(val);
         return n;
      }

      public static RealCnst MkNumeric(double val)
      {
         var n = new RealCnst();
         n.Value = new Rational(val);
         return n;
      }

      public static RealCnst MkNumeric(Rational val)
      {
         var n = new RealCnst();
         n.Value = val;
         return n;
      }

      public static StringCnst MkString(string val = default(string))
      {
         var n = new StringCnst();
         n.Value = val;
         return n;
      }

      public static Quotation MkQuotation(string val = default(string))
      {
         var n = new Quotation();
         n.Value = val;
         return n;
      }

      public static UserCnst MkUserCnst(Formula_Root.UserCnstKind val)
      {
         var n = new UserCnst();
         n.Value = val;
         return n;
      }

      public static UserCnst MkUserCnst(Formula_Root.TypeCnstKind val)
      {
         var n = new UserCnst();
         n.Value = val;
         return n;
      }

      public static UserCnst MkUserCnst(Formula_Root.Formula.UserCnstKind val)
      {
         var n = new UserCnst();
         n.Value = val;
         return n;
      }

      public static UserCnst MkUserCnst(Formula_Root.Formula.TypeCnstKind val)
      {
         var n = new UserCnst();
         n.Value = val;
         return n;
      }

      public static Formula_Root.Argument MkArgument(Formula_Root.IArgType_Argument__0 label = null, Formula_Root.IArgType_Argument__1 type = null)
      {
         var _n_ = new Formula_Root.Argument();
         if (label != null)
         {
            _n_.label = label;
         }

         if (type != null)
         {
            _n_.type = type;
         }

         return _n_;
      }

      public static Formula_Root.Arguments MkArguments(Formula_Root.IArgType_Arguments__0 cur = null, Formula_Root.IArgType_Arguments__1 index = null, Formula_Root.IArgType_Arguments__2 nxt = null)
      {
         var _n_ = new Formula_Root.Arguments();
         if (cur != null)
         {
            _n_.cur = cur;
         }

         if (index != null)
         {
            _n_.index = index;
         }

         if (nxt != null)
         {
            _n_.nxt = nxt;
         }

         return _n_;
      }

      public static Formula_Root.BaseType MkBaseType(Formula_Root.IArgType_BaseType__0 name = null, Formula_Root.IArgType_BaseType__1 argus = null)
      {
         var _n_ = new Formula_Root.BaseType();
         if (name != null)
         {
            _n_.name = name;
         }

         if (argus != null)
         {
            _n_.argus = argus;
         }

         return _n_;
      }

      public static Formula_Root.BinaryExpr1 MkBinaryExpr1(Formula_Root.IArgType_BinaryExpr1__0 op = null, Formula_Root.IArgType_BinaryExpr1__1 left = null, Formula_Root.IArgType_BinaryExpr1__2 right = null)
      {
         var _n_ = new Formula_Root.BinaryExpr1();
         if (op != null)
         {
            _n_.op = op;
         }

         if (left != null)
         {
            _n_.left = left;
         }

         if (right != null)
         {
            _n_.right = right;
         }

         return _n_;
      }

      public static Formula_Root.BinaryExpr2 MkBinaryExpr2(Formula_Root.IArgType_BinaryExpr2__0 op = null, Formula_Root.IArgType_BinaryExpr2__1 left = null, Formula_Root.IArgType_BinaryExpr2__2 right = null)
      {
         var _n_ = new Formula_Root.BinaryExpr2();
         if (op != null)
         {
            _n_.op = op;
         }

         if (left != null)
         {
            _n_.left = left;
         }

         if (right != null)
         {
            _n_.right = right;
         }

         return _n_;
      }

      public static Formula_Root.BinaryExpr3 MkBinaryExpr3(Formula_Root.IArgType_BinaryExpr3__0 op = null, Formula_Root.IArgType_BinaryExpr3__1 left = null, Formula_Root.IArgType_BinaryExpr3__2 right = null)
      {
         var _n_ = new Formula_Root.BinaryExpr3();
         if (op != null)
         {
            _n_.op = op;
         }

         if (left != null)
         {
            _n_.left = left;
         }

         if (right != null)
         {
            _n_.right = right;
         }

         return _n_;
      }

      public static Formula_Root.Body MkBody(Formula_Root.IArgType_Body__0 cur = null, Formula_Root.IArgType_Body__1 index = null, Formula_Root.IArgType_Body__2 nxt = null)
      {
         var _n_ = new Formula_Root.Body();
         if (cur != null)
         {
            _n_.cur = cur;
         }

         if (index != null)
         {
            _n_.index = index;
         }

         if (nxt != null)
         {
            _n_.nxt = nxt;
         }

         return _n_;
      }

      public static Formula_Root.BoolExpr MkBoolExpr(Formula_Root.IArgType_BoolExpr__0 label = null)
      {
         var _n_ = new Formula_Root.BoolExpr();
         if (label != null)
         {
            _n_.label = label;
         }

         return _n_;
      }

      public static Formula_Root.Count MkCount(Formula_Root.IArgType_Count__0 label = null, Formula_Root.IArgType_Count__1 body = null)
      {
         var _n_ = new Formula_Root.Count();
         if (label != null)
         {
            _n_.label = label;
         }

         if (body != null)
         {
            _n_.body = body;
         }

         return _n_;
      }

      public static Formula_Root.Domain MkDomain(Formula_Root.IArgType_Domain__0 arg_0 = null)
      {
         var _n_ = new Formula_Root.Domain();
         if (arg_0 != null)
         {
            _n_._0 = arg_0;
         }

         return _n_;
      }

      public static Formula_Root.EnumList MkEnumList(Formula_Root.IArgType_EnumList__0 cur = null, Formula_Root.IArgType_EnumList__1 nxt = null)
      {
         var _n_ = new Formula_Root.EnumList();
         if (cur != null)
         {
            _n_.cur = cur;
         }

         if (nxt != null)
         {
            _n_.nxt = nxt;
         }

         return _n_;
      }

      public static Formula_Root.EnumType MkEnumType(Formula_Root.IArgType_EnumType__0 name = null, Formula_Root.IArgType_EnumType__1 list = null)
      {
         var _n_ = new Formula_Root.EnumType();
         if (name != null)
         {
            _n_.name = name;
         }

         if (list != null)
         {
            _n_.list = list;
         }

         return _n_;
      }

      public static Formula_Root.Head MkHead(Formula_Root.IArgType_Head__0 cur = null, Formula_Root.IArgType_Head__1 index = null, Formula_Root.IArgType_Head__2 nxt = null)
      {
         var _n_ = new Formula_Root.Head();
         if (cur != null)
         {
            _n_.cur = cur;
         }

         if (index != null)
         {
            _n_.index = index;
         }

         if (nxt != null)
         {
            _n_.nxt = nxt;
         }

         return _n_;
      }

      public static Formula_Root.Label MkLabel(Formula_Root.IArgType_Label__0 name = null, Formula_Root.IArgType_Label__1 type = null, Formula_Root.IArgType_Label__2 fragments = null)
      {
         var _n_ = new Formula_Root.Label();
         if (name != null)
         {
            _n_.name = name;
         }

         if (type != null)
         {
            _n_.type = type;
         }

         if (fragments != null)
         {
            _n_.fragments = fragments;
         }

         return _n_;
      }

      public static Formula_Root.Labels MkLabels(Formula_Root.IArgType_Labels__0 cur = null, Formula_Root.IArgType_Labels__1 index = null, Formula_Root.IArgType_Labels__2 nxt = null)
      {
         var _n_ = new Formula_Root.Labels();
         if (cur != null)
         {
            _n_.cur = cur;
         }

         if (index != null)
         {
            _n_.index = index;
         }

         if (nxt != null)
         {
            _n_.nxt = nxt;
         }

         return _n_;
      }

      public static Formula_Root.Max MkMax(Formula_Root.IArgType_Max__0 x = null, Formula_Root.IArgType_Max__1 y = null)
      {
         var _n_ = new Formula_Root.Max();
         if (x != null)
         {
            _n_.x = x;
         }

         if (y != null)
         {
            _n_.y = y;
         }

         return _n_;
      }

      public static Formula_Root.Min MkMin(Formula_Root.IArgType_Min__0 x = null, Formula_Root.IArgType_Min__1 y = null)
      {
         var _n_ = new Formula_Root.Min();
         if (x != null)
         {
            _n_.x = x;
         }

         if (y != null)
         {
            _n_.y = y;
         }

         return _n_;
      }

      public static Formula_Root.ModelExpr MkModelExpr(Formula_Root.IArgType_ModelExpr__0 type = null, Formula_Root.IArgType_ModelExpr__1 labels = null)
      {
         var _n_ = new Formula_Root.ModelExpr();
         if (type != null)
         {
            _n_.type = type;
         }

         if (labels != null)
         {
            _n_.labels = labels;
         }

         return _n_;
      }

      public static Formula_Root.Rule MkRule(Formula_Root.IArgType_Rule__0 name = null, Formula_Root.IArgType_Rule__1 head = null, Formula_Root.IArgType_Rule__2 body = null)
      {
         var _n_ = new Formula_Root.Rule();
         if (name != null)
         {
            _n_.name = name;
         }

         if (head != null)
         {
            _n_.head = head;
         }

         if (body != null)
         {
            _n_.body = body;
         }

         return _n_;
      }

      public static Formula_Root.UnionList MkUnionList(Formula_Root.IArgType_UnionList__0 cur = null, Formula_Root.IArgType_UnionList__1 nxt = null)
      {
         var _n_ = new Formula_Root.UnionList();
         if (cur != null)
         {
            _n_.cur = cur;
         }

         if (nxt != null)
         {
            _n_.nxt = nxt;
         }

         return _n_;
      }

      public static Formula_Root.UnionType MkUnionType(Formula_Root.IArgType_UnionType__0 name = null, Formula_Root.IArgType_UnionType__1 list = null)
      {
         var _n_ = new Formula_Root.UnionType();
         if (name != null)
         {
            _n_.name = name;
         }

         if (list != null)
         {
            _n_.list = list;
         }

         return _n_;
      }

      public abstract partial class GroundTerm :
         ICSharpTerm
      {
         protected SpinLock rwLock = new SpinLock();
         Span span = default(Span);
         public Span Span { get { return Get<Span>(() => span); } set { Set(() => { span = value; }); } }
         public abstract int Arity { get; }
         public abstract object Symbol { get; }
         public abstract ICSharpTerm this[int index] { get; }
         protected T Get<T>(Func<T> getter)
         {
            bool gotLock = false;
            try
            {
               rwLock.Enter(ref gotLock);
               return getter();
            }
            finally
            {
               if (gotLock)
               {
                  rwLock.Exit();
               }
            }
         }

         protected void Set(System.Action setter)
         {
            bool gotLock = false;
            try
            {
               rwLock.Enter(ref gotLock);
               setter();
            }
            finally
            {
               if (gotLock)
               {
                  rwLock.Exit();
               }
            }
         }
      }

      public interface IArgType_Argument__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_Argument__1 :
         ICSharpTerm
      {
      }

      public partial class Argument :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Arguments__0
      {
         private Formula_Root.IArgType_Argument__0 _0_val = MkUserCnst(Formula_Root.UserCnstKind.NIL);
         private Formula_Root.IArgType_Argument__1 _1_val = new Formula_Root.BaseType();

         public Formula_Root.IArgType_Argument__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Argument__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Argument__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Argument__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }


         public Formula_Root.IArgType_Argument__0 label
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Argument__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Argument__1 type
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Argument__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public override int Arity { get { return 2; } }
         public override object Symbol { get { return "Argument"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_Arguments__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_Arguments__1 :
         ICSharpTerm
      {
      }

      public interface IArgType_Arguments__2 :
         ICSharpTerm
      {
      }

      public partial class Arguments :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Arguments__2,
         Formula_Root.IArgType_BaseType__1
      {
         private Formula_Root.IArgType_Arguments__0 _0_val = new Formula_Root.Argument();
         private Formula_Root.IArgType_Arguments__1 _1_val = MkNumeric(new Rational(BigInteger.Parse("0"), BigInteger.Parse("1")));
         private Formula_Root.IArgType_Arguments__2 _2_val = MkUserCnst(Formula_Root.UserCnstKind.NIL);

         public Formula_Root.IArgType_Arguments__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Arguments__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Arguments__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Arguments__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_Arguments__2 _2
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_Arguments__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }


         public Formula_Root.IArgType_Arguments__0 cur
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Arguments__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Arguments__1 index
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Arguments__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_Arguments__2 nxt
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_Arguments__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }

         public override int Arity { get { return 3; } }
         public override object Symbol { get { return "Arguments"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        case 2:
                           return _2_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_BaseType__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_BaseType__1 :
         ICSharpTerm
      {
      }

      public partial class BaseType :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Argument__1,
         Formula_Root.IArgType_Label__1,
         Formula_Root.IArgType_ModelExpr__0,
         Formula_Root.IArgType_UnionList__0,
         Formula_Root.Type
      {
         private Formula_Root.IArgType_BaseType__0 _0_val = MkString("");
         private Formula_Root.IArgType_BaseType__1 _1_val = MkUserCnst(Formula_Root.UserCnstKind.NIL);

         public Formula_Root.IArgType_BaseType__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_BaseType__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_BaseType__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_BaseType__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }


         public Formula_Root.IArgType_BaseType__0 name
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_BaseType__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_BaseType__1 argus
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_BaseType__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public override int Arity { get { return 2; } }
         public override object Symbol { get { return "BaseType"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_BinaryExpr1__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_BinaryExpr1__1 :
         ICSharpTerm
      {
      }

      public interface IArgType_BinaryExpr1__2 :
         ICSharpTerm
      {
      }

      public partial class BinaryExpr1 :
         GroundTerm,
         Formula_Root.Expr,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Body__0
      {
         private Formula_Root.IArgType_BinaryExpr1__0 _0_val = MkUserCnst(Formula_Root.UserCnstKind.EQ);
         private Formula_Root.IArgType_BinaryExpr1__1 _1_val = new Formula_Root.Label();
         private Formula_Root.IArgType_BinaryExpr1__2 _2_val = new Formula_Root.Label();

         public Formula_Root.IArgType_BinaryExpr1__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr1__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_BinaryExpr1__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr1__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_BinaryExpr1__2 _2
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr1__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }


         public Formula_Root.IArgType_BinaryExpr1__0 op
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr1__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_BinaryExpr1__1 left
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr1__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_BinaryExpr1__2 right
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr1__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }

         public override int Arity { get { return 3; } }
         public override object Symbol { get { return "BinaryExpr1"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        case 2:
                           return _2_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_BinaryExpr2__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_BinaryExpr2__1 :
         ICSharpTerm
      {
      }

      public interface IArgType_BinaryExpr2__2 :
         ICSharpTerm
      {
      }

      public partial class BinaryExpr2 :
         GroundTerm,
         Formula_Root.Expr,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Body__0
      {
         private Formula_Root.IArgType_BinaryExpr2__0 _0_val = MkUserCnst(Formula_Root.UserCnstKind.EQ);
         private Formula_Root.IArgType_BinaryExpr2__1 _1_val = new Formula_Root.Label();
         private Formula_Root.IArgType_BinaryExpr2__2 _2_val = MkUserCnst(Formula_Root.UserCnstKind.NIL);

         public Formula_Root.IArgType_BinaryExpr2__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr2__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_BinaryExpr2__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr2__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_BinaryExpr2__2 _2
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr2__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }


         public Formula_Root.IArgType_BinaryExpr2__0 op
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr2__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_BinaryExpr2__1 left
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr2__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_BinaryExpr2__2 right
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr2__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }

         public override int Arity { get { return 3; } }
         public override object Symbol { get { return "BinaryExpr2"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        case 2:
                           return _2_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_BinaryExpr3__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_BinaryExpr3__1 :
         ICSharpTerm
      {
      }

      public interface IArgType_BinaryExpr3__2 :
         ICSharpTerm
      {
      }

      public partial class BinaryExpr3 :
         GroundTerm,
         Formula_Root.Expr,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Body__0
      {
         private Formula_Root.IArgType_BinaryExpr3__0 _0_val = MkUserCnst(Formula_Root.UserCnstKind.EQ);
         private Formula_Root.IArgType_BinaryExpr3__1 _1_val = new Formula_Root.Count();
         private Formula_Root.IArgType_BinaryExpr3__2 _2_val = MkNumeric(new Rational(BigInteger.Parse("0"), BigInteger.Parse("1")));

         public Formula_Root.IArgType_BinaryExpr3__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr3__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_BinaryExpr3__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr3__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_BinaryExpr3__2 _2
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr3__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }


         public Formula_Root.IArgType_BinaryExpr3__0 op
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr3__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_BinaryExpr3__1 left
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr3__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_BinaryExpr3__2 right
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_BinaryExpr3__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }

         public override int Arity { get { return 3; } }
         public override object Symbol { get { return "BinaryExpr3"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        case 2:
                           return _2_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_Body__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_Body__1 :
         ICSharpTerm
      {
      }

      public interface IArgType_Body__2 :
         ICSharpTerm
      {
      }

      public partial class Body :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Body__2,
         Formula_Root.IArgType_Count__1,
         Formula_Root.IArgType_Rule__2
      {
         private Formula_Root.IArgType_Body__0 _0_val = new Formula_Root.BoolExpr();
         private Formula_Root.IArgType_Body__1 _1_val = MkNumeric(new Rational(BigInteger.Parse("0"), BigInteger.Parse("1")));
         private Formula_Root.IArgType_Body__2 _2_val = MkUserCnst(Formula_Root.UserCnstKind.NIL);

         public Formula_Root.IArgType_Body__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Body__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Body__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Body__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_Body__2 _2
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_Body__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }


         public Formula_Root.IArgType_Body__0 cur
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Body__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Body__1 index
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Body__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_Body__2 nxt
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_Body__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }

         public override int Arity { get { return 3; } }
         public override object Symbol { get { return "Body"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        case 2:
                           return _2_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_BoolExpr__0 :
         ICSharpTerm
      {
      }

      public partial class BoolExpr :
         GroundTerm,
         Formula_Root.Expr,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Body__0,
         Formula_Root.IArgType_Head__0
      {
         private Formula_Root.IArgType_BoolExpr__0 _0_val = new Formula_Root.Label();

         public Formula_Root.IArgType_BoolExpr__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_BoolExpr__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }


         public Formula_Root.IArgType_BoolExpr__0 label
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_BoolExpr__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public override int Arity { get { return 1; } }
         public override object Symbol { get { return "BoolExpr"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface Boolean :
         ICSharpTerm
      {
      }

      public interface IArgType_Count__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_Count__1 :
         ICSharpTerm
      {
      }

      public partial class Count :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_BinaryExpr3__1,
         Formula_Root.InterpretedFunc
      {
         private Formula_Root.IArgType_Count__0 _0_val = new Formula_Root.Label();
         private Formula_Root.IArgType_Count__1 _1_val = new Formula_Root.Body();

         public Formula_Root.IArgType_Count__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Count__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Count__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Count__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }


         public Formula_Root.IArgType_Count__0 label
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Count__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Count__1 body
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Count__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public override int Arity { get { return 2; } }
         public override object Symbol { get { return "Count"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_Domain__0 :
         ICSharpTerm
      {
      }

      public partial class Domain :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data
      {
         private Formula_Root.IArgType_Domain__0 _0_val = MkString("");

         public Formula_Root.IArgType_Domain__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Domain__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }


         public override int Arity { get { return 1; } }
         public override object Symbol { get { return "Domain"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_EnumList__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_EnumList__1 :
         ICSharpTerm
      {
      }

      public partial class EnumList :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_EnumList__1,
         Formula_Root.IArgType_EnumType__1
      {
         private Formula_Root.IArgType_EnumList__0 _0_val = MkUserCnst(Formula_Root.UserCnstKind.NIL);
         private Formula_Root.IArgType_EnumList__1 _1_val = MkUserCnst(Formula_Root.UserCnstKind.NIL);

         public Formula_Root.IArgType_EnumList__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_EnumList__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_EnumList__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_EnumList__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }


         public Formula_Root.IArgType_EnumList__0 cur
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_EnumList__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_EnumList__1 nxt
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_EnumList__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public override int Arity { get { return 2; } }
         public override object Symbol { get { return "EnumList"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_EnumType__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_EnumType__1 :
         ICSharpTerm
      {
      }

      public partial class EnumType :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Argument__1,
         Formula_Root.IArgType_Label__1,
         Formula_Root.IArgType_ModelExpr__0,
         Formula_Root.IArgType_UnionList__0,
         Formula_Root.Type
      {
         private Formula_Root.IArgType_EnumType__0 _0_val = MkString("");
         private Formula_Root.IArgType_EnumType__1 _1_val = new Formula_Root.EnumList();

         public Formula_Root.IArgType_EnumType__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_EnumType__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_EnumType__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_EnumType__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }


         public Formula_Root.IArgType_EnumType__0 name
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_EnumType__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_EnumType__1 list
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_EnumType__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public override int Arity { get { return 2; } }
         public override object Symbol { get { return "EnumType"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface Expr :
         ICSharpTerm
      {
      }

      public interface FormulaBuiltInType :
         ICSharpTerm
      {
      }

      public interface IArgType_Head__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_Head__1 :
         ICSharpTerm
      {
      }

      public interface IArgType_Head__2 :
         ICSharpTerm
      {
      }

      public partial class Head :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Head__2,
         Formula_Root.IArgType_Rule__1
      {
         private Formula_Root.IArgType_Head__0 _0_val = new Formula_Root.BoolExpr();
         private Formula_Root.IArgType_Head__1 _1_val = MkNumeric(new Rational(BigInteger.Parse("0"), BigInteger.Parse("1")));
         private Formula_Root.IArgType_Head__2 _2_val = MkUserCnst(Formula_Root.UserCnstKind.NIL);

         public Formula_Root.IArgType_Head__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Head__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Head__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Head__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_Head__2 _2
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_Head__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }


         public Formula_Root.IArgType_Head__0 cur
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Head__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Head__1 index
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Head__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_Head__2 nxt
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_Head__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }

         public override int Arity { get { return 3; } }
         public override object Symbol { get { return "Head"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        case 2:
                           return _2_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface Integer :
         ICSharpTerm
      {
      }

      public interface InterpretedFunc :
         ICSharpTerm
      {
      }

      public interface IArgType_Label__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_Label__1 :
         ICSharpTerm
      {
      }

      public interface IArgType_Label__2 :
         ICSharpTerm
      {
      }

      public partial class Label :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_BinaryExpr1__1,
         Formula_Root.IArgType_BinaryExpr1__2,
         Formula_Root.IArgType_BinaryExpr2__1,
         Formula_Root.IArgType_BinaryExpr3__2,
         Formula_Root.IArgType_BoolExpr__0,
         Formula_Root.IArgType_Count__0,
         Formula_Root.IArgType_Labels__0
      {
         private Formula_Root.IArgType_Label__0 _0_val = MkString("");
         private Formula_Root.IArgType_Label__1 _1_val = MkUserCnst(Formula_Root.UserCnstKind.NIL);
         private Formula_Root.IArgType_Label__2 _2_val = MkUserCnst(Formula_Root.UserCnstKind.NIL);

         public Formula_Root.IArgType_Label__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Label__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Label__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Label__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_Label__2 _2
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_Label__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }


         public Formula_Root.IArgType_Label__0 name
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Label__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Label__1 type
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Label__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_Label__2 fragments
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_Label__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }

         public override int Arity { get { return 3; } }
         public override object Symbol { get { return "Label"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        case 2:
                           return _2_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_Labels__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_Labels__1 :
         ICSharpTerm
      {
      }

      public interface IArgType_Labels__2 :
         ICSharpTerm
      {
      }

      public partial class Labels :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Label__2,
         Formula_Root.IArgType_Labels__2,
         Formula_Root.IArgType_ModelExpr__1
      {
         private Formula_Root.IArgType_Labels__0 _0_val = new Formula_Root.Label();
         private Formula_Root.IArgType_Labels__1 _1_val = MkNumeric(new Rational(BigInteger.Parse("0"), BigInteger.Parse("1")));
         private Formula_Root.IArgType_Labels__2 _2_val = MkUserCnst(Formula_Root.UserCnstKind.NIL);

         public Formula_Root.IArgType_Labels__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Labels__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Labels__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Labels__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_Labels__2 _2
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_Labels__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }


         public Formula_Root.IArgType_Labels__0 cur
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Labels__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Labels__1 index
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Labels__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_Labels__2 nxt
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_Labels__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }

         public override int Arity { get { return 3; } }
         public override object Symbol { get { return "Labels"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        case 2:
                           return _2_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_Max__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_Max__1 :
         ICSharpTerm
      {
      }

      public partial class Max :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.InterpretedFunc
      {
         private Formula_Root.IArgType_Max__0 _0_val = MkNumeric(new Rational(BigInteger.Parse("0"), BigInteger.Parse("1")));
         private Formula_Root.IArgType_Max__1 _1_val = MkNumeric(new Rational(BigInteger.Parse("0"), BigInteger.Parse("1")));

         public Formula_Root.IArgType_Max__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Max__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Max__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Max__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }


         public Formula_Root.IArgType_Max__0 x
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Max__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Max__1 y
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Max__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public override int Arity { get { return 2; } }
         public override object Symbol { get { return "Max"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_Min__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_Min__1 :
         ICSharpTerm
      {
      }

      public partial class Min :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.InterpretedFunc
      {
         private Formula_Root.IArgType_Min__0 _0_val = MkNumeric(new Rational(BigInteger.Parse("0"), BigInteger.Parse("1")));
         private Formula_Root.IArgType_Min__1 _1_val = MkNumeric(new Rational(BigInteger.Parse("0"), BigInteger.Parse("1")));

         public Formula_Root.IArgType_Min__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Min__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Min__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Min__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }


         public Formula_Root.IArgType_Min__0 x
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Min__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Min__1 y
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Min__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public override int Arity { get { return 2; } }
         public override object Symbol { get { return "Min"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_ModelExpr__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_ModelExpr__1 :
         ICSharpTerm
      {
      }

      public partial class ModelExpr :
         GroundTerm,
         Formula_Root.Expr,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Body__0,
         Formula_Root.IArgType_Head__0
      {
         private Formula_Root.IArgType_ModelExpr__0 _0_val = new Formula_Root.BaseType();
         private Formula_Root.IArgType_ModelExpr__1 _1_val = new Formula_Root.Labels();

         public Formula_Root.IArgType_ModelExpr__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_ModelExpr__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_ModelExpr__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_ModelExpr__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }


         public Formula_Root.IArgType_ModelExpr__0 type
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_ModelExpr__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_ModelExpr__1 labels
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_ModelExpr__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public override int Arity { get { return 2; } }
         public override object Symbol { get { return "ModelExpr"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface Natural :
         ICSharpTerm
      {
      }

      public interface NegInteger :
         ICSharpTerm
      {
      }

      public interface Operator :
         ICSharpTerm
      {
      }

      public interface PosInteger :
         ICSharpTerm
      {
      }

      public interface Real :
         ICSharpTerm
      {
      }

      public interface IArgType_Rule__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_Rule__1 :
         ICSharpTerm
      {
      }

      public interface IArgType_Rule__2 :
         ICSharpTerm
      {
      }

      public partial class Rule :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data
      {
         private Formula_Root.IArgType_Rule__0 _0_val = MkString("");
         private Formula_Root.IArgType_Rule__1 _1_val = new Formula_Root.Head();
         private Formula_Root.IArgType_Rule__2 _2_val = new Formula_Root.Body();

         public Formula_Root.IArgType_Rule__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Rule__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Rule__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Rule__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_Rule__2 _2
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_Rule__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }


         public Formula_Root.IArgType_Rule__0 name
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_Rule__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_Rule__1 head
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_Rule__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public Formula_Root.IArgType_Rule__2 body
         {
            get
            {
               Contract.Ensures(_2_val != null);
               return Get<Formula_Root.IArgType_Rule__2>(() => _2_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _2_val = value; });
            }
         }

         public override int Arity { get { return 3; } }
         public override object Symbol { get { return "Rule"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        case 2:
                           return _2_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface String :
         ICSharpTerm
      {
      }

      public interface Type :
         ICSharpTerm
      {
      }

      public interface IArgType_UnionList__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_UnionList__1 :
         ICSharpTerm
      {
      }

      public partial class UnionList :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_UnionList__1,
         Formula_Root.IArgType_UnionType__1
      {
         private Formula_Root.IArgType_UnionList__0 _0_val = new Formula_Root.BaseType();
         private Formula_Root.IArgType_UnionList__1 _1_val = MkUserCnst(Formula_Root.UserCnstKind.NIL);

         public Formula_Root.IArgType_UnionList__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_UnionList__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_UnionList__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_UnionList__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }


         public Formula_Root.IArgType_UnionList__0 cur
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_UnionList__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_UnionList__1 nxt
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_UnionList__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public override int Arity { get { return 2; } }
         public override object Symbol { get { return "UnionList"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public interface IArgType_UnionType__0 :
         ICSharpTerm
      {
      }

      public interface IArgType_UnionType__1 :
         ICSharpTerm
      {
      }

      public partial class UnionType :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Data,
         Formula_Root.IArgType_Argument__1,
         Formula_Root.IArgType_Label__1,
         Formula_Root.IArgType_ModelExpr__0,
         Formula_Root.Type
      {
         private Formula_Root.IArgType_UnionType__0 _0_val = MkString("");
         private Formula_Root.IArgType_UnionType__1 _1_val = new Formula_Root.UnionList();

         public Formula_Root.IArgType_UnionType__0 _0
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_UnionType__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_UnionType__1 _1
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_UnionType__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }


         public Formula_Root.IArgType_UnionType__0 name
         {
            get
            {
               Contract.Ensures(_0_val != null);
               return Get<Formula_Root.IArgType_UnionType__0>(() => _0_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _0_val = value; });
            }
         }

         public Formula_Root.IArgType_UnionType__1 list
         {
            get
            {
               Contract.Ensures(_1_val != null);
               return Get<Formula_Root.IArgType_UnionType__1>(() => _1_val);
            }

            set
            {
               Contract.Requires(value != null);
               Set(() => { _1_val = value; });
            }
         }

         public override int Arity { get { return 2; } }
         public override object Symbol { get { return "UnionType"; } }
         public override ICSharpTerm this[int index]
         {
            get
            {
               return Get<ICSharpTerm>(
                  () =>
                  {
                     switch (index)
                     {
                        case 0:
                           return _0_val;
                        case 1:
                           return _1_val;
                        default:
                           throw new InvalidOperationException();
                     }
                  }
               );
            }
         }
      }

      public partial class RealCnst :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Constant,
         Formula_Root.Formula.Data,
         Formula_Root.FormulaBuiltInType,
         Formula_Root.IArgType_Arguments__1,
         Formula_Root.IArgType_BinaryExpr2__2,
         Formula_Root.IArgType_BinaryExpr3__2,
         Formula_Root.IArgType_Body__1,
         Formula_Root.IArgType_EnumList__0,
         Formula_Root.IArgType_Head__1,
         Formula_Root.IArgType_Labels__1,
         Formula_Root.IArgType_Max__0,
         Formula_Root.IArgType_Max__1,
         Formula_Root.IArgType_Min__0,
         Formula_Root.IArgType_Min__1,
         Formula_Root.Integer,
         Formula_Root.Natural,
         Formula_Root.NegInteger,
         Formula_Root.PosInteger,
         Formula_Root.Real
      {
         Rational val = default(Rational);
         public override int Arity { get { return 0; } }
         public override object Symbol { get { return Get<Rational>(() => val); } }
         public override ICSharpTerm this[int index] { get { throw new InvalidOperationException(); } }
         public Rational Value { get { return Get<Rational>(() => val); } set { Set(() => { val = value; }); } }
      }

      public partial class StringCnst :
         GroundTerm,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Constant,
         Formula_Root.Formula.Data,
         Formula_Root.FormulaBuiltInType,
         Formula_Root.IArgType_Argument__0,
         Formula_Root.IArgType_BaseType__0,
         Formula_Root.IArgType_BinaryExpr2__2,
         Formula_Root.IArgType_Domain__0,
         Formula_Root.IArgType_EnumList__0,
         Formula_Root.IArgType_EnumType__0,
         Formula_Root.IArgType_Label__0,
         Formula_Root.IArgType_Rule__0,
         Formula_Root.IArgType_UnionType__0,
         Formula_Root.String
      {
         string val = default(string);
         public override int Arity { get { return 0; } }
         public override object Symbol { get { return Get<string>(() => val); } }
         public override ICSharpTerm this[int index] { get { throw new InvalidOperationException(); } }
         public string Value { get { return Get<string>(() => val); } set { Set(() => { val = value; }); } }
      }

      public partial class Quotation :
         GroundTerm,
         Formula_Root.Boolean,
         Formula_Root.Expr,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Constant,
         Formula_Root.Formula.Data,
         Formula_Root.FormulaBuiltInType,
         Formula_Root.IArgType_Argument__0,
         Formula_Root.IArgType_Argument__1,
         Formula_Root.IArgType_Arguments__0,
         Formula_Root.IArgType_Arguments__1,
         Formula_Root.IArgType_Arguments__2,
         Formula_Root.IArgType_BaseType__0,
         Formula_Root.IArgType_BaseType__1,
         Formula_Root.IArgType_BinaryExpr1__0,
         Formula_Root.IArgType_BinaryExpr1__1,
         Formula_Root.IArgType_BinaryExpr1__2,
         Formula_Root.IArgType_BinaryExpr2__0,
         Formula_Root.IArgType_BinaryExpr2__1,
         Formula_Root.IArgType_BinaryExpr2__2,
         Formula_Root.IArgType_BinaryExpr3__0,
         Formula_Root.IArgType_BinaryExpr3__1,
         Formula_Root.IArgType_BinaryExpr3__2,
         Formula_Root.IArgType_Body__0,
         Formula_Root.IArgType_Body__1,
         Formula_Root.IArgType_Body__2,
         Formula_Root.IArgType_BoolExpr__0,
         Formula_Root.IArgType_Count__0,
         Formula_Root.IArgType_Count__1,
         Formula_Root.IArgType_Domain__0,
         Formula_Root.IArgType_EnumList__0,
         Formula_Root.IArgType_EnumList__1,
         Formula_Root.IArgType_EnumType__0,
         Formula_Root.IArgType_EnumType__1,
         Formula_Root.IArgType_Head__0,
         Formula_Root.IArgType_Head__1,
         Formula_Root.IArgType_Head__2,
         Formula_Root.IArgType_Label__0,
         Formula_Root.IArgType_Label__1,
         Formula_Root.IArgType_Label__2,
         Formula_Root.IArgType_Labels__0,
         Formula_Root.IArgType_Labels__1,
         Formula_Root.IArgType_Labels__2,
         Formula_Root.IArgType_Max__0,
         Formula_Root.IArgType_Max__1,
         Formula_Root.IArgType_Min__0,
         Formula_Root.IArgType_Min__1,
         Formula_Root.IArgType_ModelExpr__0,
         Formula_Root.IArgType_ModelExpr__1,
         Formula_Root.IArgType_Rule__0,
         Formula_Root.IArgType_Rule__1,
         Formula_Root.IArgType_Rule__2,
         Formula_Root.IArgType_UnionList__0,
         Formula_Root.IArgType_UnionList__1,
         Formula_Root.IArgType_UnionType__0,
         Formula_Root.IArgType_UnionType__1,
         Formula_Root.Integer,
         Formula_Root.InterpretedFunc,
         Formula_Root.Natural,
         Formula_Root.NegInteger,
         Formula_Root.Operator,
         Formula_Root.PosInteger,
         Formula_Root.Real,
         Formula_Root.String,
         Formula_Root.Type
      {
         string val = string.Empty;
         public override int Arity { get { return 0; } }
         public override object Symbol { get { return Get<string>(() => string.Format("`{0}`", val)); } }
         public override ICSharpTerm this[int index] { get { throw new InvalidOperationException(); } }
         public string Value { get { return Get<string>(() => val); } set { Set(() => { val = value; }); } }
      }

      public partial class UserCnst :
         GroundTerm,
         Formula_Root.Boolean,
         Formula_Root.Formula.Any,
         Formula_Root.Formula.Constant,
         Formula_Root.Formula.Data,
         Formula_Root.FormulaBuiltInType,
         Formula_Root.IArgType_Argument__0,
         Formula_Root.IArgType_Arguments__2,
         Formula_Root.IArgType_BaseType__1,
         Formula_Root.IArgType_BinaryExpr1__0,
         Formula_Root.IArgType_BinaryExpr2__0,
         Formula_Root.IArgType_BinaryExpr2__2,
         Formula_Root.IArgType_BinaryExpr3__0,
         Formula_Root.IArgType_Body__2,
         Formula_Root.IArgType_EnumList__0,
         Formula_Root.IArgType_EnumList__1,
         Formula_Root.IArgType_Head__2,
         Formula_Root.IArgType_Label__1,
         Formula_Root.IArgType_Label__2,
         Formula_Root.IArgType_Labels__2,
         Formula_Root.IArgType_UnionList__1,
         Formula_Root.Operator
      {
         private object val = Formula_Root.UserCnstKind.FALSE;
         public override int Arity { get { return 0; } }
         public override object Symbol { get { return Get<object>(() => ToSymbol(val)); } }
         public override ICSharpTerm this[int index] { get { throw new InvalidOperationException(); } }
         public object Value
         {
            get
            {
               return Get<object>(() => val);
            }

            set
            {
               if (!ValidateType(value))
               {
                  throw new InvalidOperationException();
               }

               Set(() => { val = value; });
            }
         }

         private static bool ValidateType(object o)
         {
            if (o == null)
            {
               return true;
            }
            else if (o is Formula_Root.UserCnstKind)
            {
               return true;
            }
            else if (o is Formula_Root.TypeCnstKind)
            {
               return true;
            }
            else if (o is Formula_Root.Formula.UserCnstKind)
            {
               return true;
            }
            else if (o is Formula_Root.Formula.TypeCnstKind)
            {
               return true;
            }
            else
            {
               return false;
            }
         }

         private static string ToSymbol(object o)
         {
            if (o == null)
            {
               return null;
            }
            else if (o is Formula_Root.UserCnstKind)
            {
               return Formula_Root.UserCnstNames[(int)o];
            }
            else if (o is Formula_Root.TypeCnstKind)
            {
               return Formula_Root.TypeCnstNames[(int)o];
            }
            else if (o is Formula_Root.Formula.UserCnstKind)
            {
               return Formula_Root.Formula.UserCnstNames[(int)o];
            }
            else if (o is Formula_Root.Formula.TypeCnstKind)
            {
               return Formula_Root.Formula.TypeCnstNames[(int)o];
            }
            else
            {
               throw new InvalidOperationException();
            }
         }
      }

      public static partial class Formula
      {
         public enum UserCnstKind
         {
         }

         public enum TypeCnstKind
         {
            Any,
            Constant,
            Data
         }

         public static readonly string[] UserCnstNames =
         {
         };

         public static readonly string[] TypeCnstNames =
         {
            "Formula.#Any",
            "Formula.#Constant",
            "Formula.#Data"
         };

         public static string Namespace { get { return "Formula"; } }
         public interface Any :
            ICSharpTerm
         {
         }

         public interface Constant :
            ICSharpTerm
         {
         }

         public interface Data :
            ICSharpTerm
         {
         }

      }

   }
}
