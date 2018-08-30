namespace Microsoft.Formula.GraphDatabaseEngine
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Reflection;
    using System.Diagnostics;

    using Microsoft.Formula.API;
    using Microsoft.Formula.API.ASTQueries;
    using Microsoft.Formula.API.Nodes;
    using Microsoft.Formula.Compiler;
    using Microsoft.Formula.Common.Terms;

    using Microsoft.Formula.GraphDatabaseEngine.Domains;

    public class Compiler
    {
        public Env CompilerEnv
        {
            get;
            private set;
        }

        public Compiler()
        {
            EnvParams envParams = null;
            CompilerEnv = new Env(envParams);
        }

        public AST<Program> ParseEmbeddedResourceProgram(string programName)
        {
            var execDir = (new FileInfo(Assembly.GetExecutingAssembly().Location)).DirectoryName;

            // programName must be set as an embedded resource file in properties settings.
            string manifestName = "GraphDatabaseEngine.Domains." + programName;
            var asm = Assembly.GetExecutingAssembly();
            var names = asm.GetManifestResourceNames();
            string programStr;
            var stream = asm.GetManifestResourceStream(manifestName);
            using (var sr = new StreamReader(asm.GetManifestResourceStream(manifestName)))
            {
                programStr = sr.ReadToEnd();
            }

            var parseTask = Factory.Instance.ParseText(new ProgramName(Path.Combine(execDir, programName)), programStr);
            if (!parseTask.Result.Succeeded)
            {
                string errorMsg = String.Format("Cannot load resources from {0}", programName);
                throw new Exception(errorMsg);
            }
            
            return parseTask.Result.Program;
        }

        public void Compile(string inputFile)
        {
            string programName = "Formula.4ml";
            var program = ParseEmbeddedResourceProgram(programName);
            InstallResult result;
            CompilerEnv.Install(program, out result);

            var translator = new GremlinTranslator(inputFile);
            foreach (var domainStoreKey in translator.DomainStores.Keys)
            {
                DomainStore domainStore = translator.DomainStores[domainStoreKey];

                domainStore.GenerateFormulaTerms();
            }

        }

        
    }
}
