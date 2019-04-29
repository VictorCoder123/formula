grammar Formula;

/*
 * Parser Rules
 */


program
   : EOF
   | config      
   | moduleList   
   | config     
	 moduleList
   ;    

moduleList
   : module      //{ EndModule(); }
   | module      //{ EndModule(); }
	 moduleList
   ;

module 
   : domain
   | model
   | transform
   | tSystem
   | machine
   ;


/**************  Machines ************/

machine
	: machineSigConfig  
	  LCBRACE			
	  RCBRACE			

	| machineSigConfig  
	  LCBRACE			//{ SetModRefState(ModRefState.ModApply); }
	  machineBody
	  RCBRACE			
   ;

machineBody
	: machineSentenceConf	 
	| machineSentenceConf
	  machineBody
	;

machineSentenceConf
    : machineSentence
	| sentenceConfig
	  machineSentence
    ;

machineSentence
	: machineProp
	| BOOT        //{ IsBuildingUpdate = false; } 
	  step
	| INITIALLY   //{ IsBuildingNext = false; IsBuildingUpdate = true; }
	  update
	| NEXT        //{ IsBuildingNext = true; IsBuildingUpdate = true;  }
	  update
	;
	  
machineProp
	: PROPERTY
	  BAREID
	  EQ
	  funcTerm
	  DOT        //{ AppendProperty($2.str, ToSpan(@1)); }
	;	

/**************  Machine Signature ************/

machineSigConfig
   : machineSig    //{ SetModRefState(ModRefState.None);  }
   | machineSig    //{ SetModRefState(ModRefState.None);  } 
	 config        
   ;

machineSig
	: MACHINE
	  BAREID         //{ StartMachine($2.str, ToSpan(@2)); }
	  machineSigIn	   
	  OF			 //{ SetModRefState(ModRefState.Other); }
	  modRefs       
	;

machineSigIn
	: LPAREN
	  RPAREN
	| LPAREN
	  voMParamList
	  RPAREN
	;


/**************  Models ************/

model
    : modelSigConfig
	  LCBRACE	  
	  RCBRACE     

    | modelSigConfig
	  LCBRACE      
	  modelBody
	  RCBRACE   
   ;

modelBody
	: modelSentence
   | modelSentence
	 modelBody
   ;

modelSentence
	 : modelFactList
	 | modelContractConf
	 ;

modelContractConf
     : modelContract
	 | sentenceConfig
	   modelContract
	 ;

modelContract
	 : ENSURES    //{ StartPropContract(ContractKind.EnsuresProp, ToSpan(@1)); }
	   bodyList
	   DOT	       

	 | REQUIRES   //{ StartPropContract(ContractKind.RequiresProp, ToSpan(@1));  }
	   bodyList
	   DOT		   

	 | REQUIRES
	   CardSpec
	   DIGITS
	   Id          //{ AppendCardContract($2.str, ParseInt($3.str, ToSpan(@3)), ToSpan(@1)); } 
	   DOT
	 ;

modelFactList
	   : modelFact   
		 DOT

       | sentenceConfig
	     modelFact   
		 DOT

	   | modelFact   
		 COMMA
		 modelFactList   

	   | sentenceConfig
	     modelFact   
		 COMMA
		 modelFactList   
	   ; 

modelFact 
	   : funcTerm    //{ AppendFact(MkFact(false, ToSpan(@1))); }
	   | BAREID      
		 IS          //{ PushArg(new Nodes.Id(ToSpan(@1), $1.str));  }
		 funcTerm    //{ AppendFact(MkFact(true, ToSpan(@1))); }
	   ;

CardSpec
	 : SOME
	 | ATMOST
	 | ATLEAST
	 ;

/**************  Model Signatures  ************/

modelSigConfig
   : modelSig			 //{ SetModRefState(ModRefState.None); }
   | modelSig			 //{ SetModRefState(ModRefState.None); }
	 Config             
   ;

modelSig
   : modelIntro
   | modelIntro
     INCLUDES  //{ SetCompose(ComposeKind.Includes); SetModRefState(ModRefState.Input); }
	 modRefs
   | modelIntro
     EXTENDS  //{ SetCompose(ComposeKind.Extends); SetModRefState(ModRefState.Input); }
	 modRefs                        
   ;

modelIntro
   : MODEL     
	 BAREID    //{ StartModel($2.str, false, ToSpan(@1)); }
	 OF
	 modRef    

   | PARTIAL   
	 MODEL     
	 BAREID    //{ StartModel($3.str, true, ToSpan(@1)); }
	 OF
	 modRef    
   ;


/**************  Transform Systems  ************/

tSystem 
	   : TRANSFORM
		 SYSTEM
	     BAREID      //{ StartTSystem($3.str, ToSpan(@1)); }
		 tSystemRest
	   ;

tSystemRest 
	   : transformSigConfig
		 LCBRACE
		 RCBRACE 

	   | transformSigConfig
		 LCBRACE     //{ IsBuildingUpdate = false; SetModRefState(ModRefState.ModApply); }
		 transSteps
		 RCBRACE  
	   ;	

transSteps
	 : transStepConfig
	 | transStepConfig
	   transSteps
	 ; 

transStepConfig
     : step
	 | sentenceConfig
	   step
     ;

/**************  Transforms  ************/

transform 
	   : TRANSFORM
	     BAREID      //{ StartTransform($2.str, ToSpan(@1)); }
		 transformRest
	   ;

transformRest
	   : transformSigConfig
		 LCBRACE
		 RCBRACE

	   | transformSigConfig
		 LCBRACE     
		 transBody
		 RCBRACE  
	   ;	

transBody
	 : transSentenceConfig
	 | transSentenceConfig
	   transBody
	 ;

transSentenceConfig
     : transSentence
	 | sentenceConfig
	   transSentence
	 ;

transSentence
	 : rule
	 | typeDecl	 
	 | ENSURES    //{ StartPropContract(ContractKind.EnsuresProp, ToSpan(@1)); }
	   bodyList
	   DOT		  
	 | REQUIRES   //{ StartPropContract(ContractKind.RequiresProp, ToSpan(@1)); }
	   bodyList
	   DOT	      
	 ;

/**************  Transform Signatures ************/

transformSigConfig
		: transformSig   //{ SetModRefState(ModRefState.None);  }
		| transformSig   //{ SetModRefState(ModRefState.None);  }
		  config         
		;

transformSig
		: transSigIn		  
		  RETURNS       		  
		  LPAREN         //{ SetModRefState(ModRefState.Output); }
		  modelParamList
		  RPAREN         
		;
 
transSigIn
		: LPAREN
		  RPAREN
		| LPAREN
		  voMParamList
		  RPAREN
		;

/**************  Domains  ************/

domain 
	   : domainSigConfig
		 LCBRACE
		 RCBRACE 

	   | domainSigConfig  
		 LCBRACE
		 domSentences
		 RCBRACE   
	   ;

domSentences 
	 : domSentenceConfig   
	 | domSentenceConfig
	   domSentences   
	 ;

domSentenceConfig
	 : domSentence
	 | dentenceConfig
	   domSentence
	 ;

domSentence 
	 : rule
	 | typeDecl
	 | CONFORMS //{ StartPropContract(ContractKind.ConformsProp, ToSpan(@1)); }
	   bodyList
	   DOT		  
	 ;

/*************** Domain Signature ***************/

domainSigConfig
		: domainSig  //{ SetModRefState(ModRefState.None);  }
		| domainSig  //{ SetModRefState(ModRefState.None);  }
		  config     
		;

domainSig 
		: DOMAIN
		  BAREID     //{ StartDomain($2.str, ComposeKind.None, ToSpan(@1)); }

		| DOMAIN
		  BAREID
		  EXTENDS    //{ StartDomain($2.str, ComposeKind.Extends, ToSpan(@1)); }
		  modRefs    

		| DOMAIN
		  BAREID
		  INCLUDES  //{ StartDomain($2.str, ComposeKind.Includes, ToSpan(@1)); }
		  modRefs       
		;

/**************  Configurations ************/

config
	   : LSBRACE
		 settingList
		 RSBRACE
	   ;

sentenceConfig
	   : LSBRACE       //{ StartSentenceConfig(ToSpan(@1)); }
		 settingList
		 RSBRACE
	   ;

settingList
	   : setting        
	   | setting
		 COMMA
		 settingList        
	   ;

setting 
	   : id
		 EQ
		 constant  //{ AppendSetting(); }
	   ;

/**************  Parameters ************/

modelParamList
	   : modRefRename
	   | modRefRename
		 COMMA
		 modelParamList
	   ;

valOrModelParam 
	   : BAREID
		 COLON
		 unnBody         //{ AppendParam($1.str, ToSpan(@1)); }
	   | modRefRename
	   ;

voMParamList
	   : valOrModelParam
	   | valOrModelParam
		 COMMA
		 voMParamList
	   ;

/**************  Steps and Updates ************/

update
	 : stepOrUpdateLHS
	   EQ
	   choiceList
	   DOT			 //{ AppendUpdate(); }
	 ;

step
	 : stepOrUpdateLHS
	   EQ
	   modApply		 
	   DOT			 //{ AppendStep(); }
	 ;

choiceList 
	: modApply       //{ AppendChoice(); }
	| modApply       //{ AppendChoice(); }
	  SEMICOLON
	  choiceList
	;

modApply
	 : modRef
	   LPAREN         
	   RPAREN         //{ PushArg(MkModApply()); }

	 | modRef
	   LPAREN         
	   modArgList
	   RPAREN         //{ PushArg(MkModApply()); }
	 ;

modArgList
	: modAppArg         //{ IncArity(); }
	| modAppArg         //{ IncArity(); }
	  COMMA
	  modArgList
	;

modAppArg 
	: funcTerm   
    | BAREID
      AT
      String          //{ PushArg(new Nodes.ModRef(ToSpan(@1), $1.str, null, GetStringValue())); }
    ;

stepOrUpdateLHS 
	 : id			  //{ AppendLHS(); }
	 | id			  //{ AppendLHS(); }
	   COMMA
	   stepOrUpdateLHS
	 ;

/**************  Module References ************/

modRefs 
		: modRef
		| modRef
		  COMMA
		  modRefs
		;

modRef 
		: modRefRename
		| modRefNoRename
		;

modRefRename 
	   : BAREID
		 RENAMES
		 BAREID    //{ AppendModRef(new Nodes.ModRef(ToSpan(@1), $3.str, $1.str, null)); }
		   
	   | BAREID
		 RENAMES
		 BAREID
		 AT
		 String    /{ AppendModRef(new Nodes.ModRef(ToSpan(@1), $3.str, $1.str, GetStringValue())); }
	   ;

modRefNoRename 
	   : BAREID    //{ AppendModRef(new Nodes.ModRef(ToSpan(@1), $1.str, null, null)); }
	   | BAREID
		 AT
		 String    //{ AppendModRef(new Nodes.ModRef(ToSpan(@1), $1.str, null, GetStringValue())); }
	   ;

/**************** Type Decls *****************/

typeDecl 
		 : BAREID        //{ SaveTypeDeclName($1.str, ToSpan(@1)); }
		   TYPEDEF      
		   typeDeclBody
		   DOT          
		 ;

typeDeclBody 
        : unnBody        //{ EndUnnDecl(); } 

		| LPAREN		 //{ StartConDecl(false, false); } 
		  fields
		  RPAREN         //{ EndTypeDecl(); }

		| SUB	         //{ StartConDecl(false, true); }
		  LPAREN
		  fields
		  RPAREN         //{ EndTypeDecl(); }

		| NEW	         //{ StartConDecl(true, false); }
		  LPAREN
		  fields
		  RPAREN         //{ EndTypeDecl(); }

		| funDecl        
		  LPAREN
		  fields
		  mapArrow       
		  fields
		  RPAREN         //{ EndTypeDecl(); }
		;		 

funDecl 
		: INJ          //{ StartMapDecl(MapKind.Inj); }
		| BIJ		   //{ StartMapDecl(MapKind.Bij); }
		| SUR		   //{ StartMapDecl(MapKind.Sur); }
		| FUN          //{ StartMapDecl(MapKind.Fun); }
		;

fields 
	   : field
	   | field
		 COMMA
		 fields
	   ;

field 
	  : unnBody         //{ AppendField(null, false, ToSpan(@1)); }
	  | ANY            
		unnBody         //{ AppendField(null, true, ToSpan(@1)); }
	  | BAREID
		COLON
		unnBody         //{ AppendField($1.str, false, ToSpan(@1)); }
	  | BAREID
		COLON
		ANY
		unnBody         //{ AppendField($1.str, true, ToSpan(@1)); }
	  ;

mapArrow 
      : WEAKARROW       //{ SaveMapPartiality(true); }
	  | STRONGARROW     //{ SaveMapPartiality(false); }
	  ;

/**************** Type Terms *****************/

unnBody 
		: unnCmp
		| unnCmp
		  PLUS
		  unnBody
		;

unnCmp 
	   : typeId
	   | LCBRACE    //{ StartEnum(ToSpan(@1)); }
		 enumList
		 RCBRACE    //{ EndEnum(); }
	   ;

typeId
	 : BAREID         //{ AppendUnion(new Nodes.Id(ToSpan(@1), $1.str)); }
	 | QUALID         //{ AppendUnion(new Nodes.Id(ToSpan(@1), $1.str)); }
	 ; 

enumList 
		 : enumCnst
		 | enumCnst
		   COMMA
		   enumList
		 ;

enumCnst
		 : DIGITS      //{ AppendEnum(ParseNumeric($1.str, false, ToSpan(@1))); }
		 | REAL        //{ AppendEnum(ParseNumeric($1.str, false, ToSpan(@1)));    }
		 | FRAC        //{ AppendEnum(ParseNumeric($1.str, true,  ToSpan(@1)));    }
		 | String      //{ AppendEnum(GetString());  }
		 | BAREID      //{ AppendEnum(new Nodes.Id(ToSpan(@1), $1.str));  }
		 | QUALID      //{ AppendEnum(new Nodes.Id(ToSpan(@1), $1.str));  }
		 | DIGITS      
		   RANGE
		   DIGITS      //{ AppendEnum(new Nodes.Range(ToSpan(@1), ParseNumeric($1.str), ParseNumeric($3.str))); }
		 ;

/************* Facts, Rules, and Comprehensions **************/

rule 
	 : funcTermList       //{ EndHeads(ToSpan(@1));   }
	   DOT                //{ AppendRule(); }
	 | funcTermList
	   RULE               //{ EndHeads(ToSpan(@1));  }
	   bodyList
	   DOT				  //{ AppendRule(); }
	 ;

compr  
	 : LCBRACE		      //{ PushComprSymbol(ToSpan(@1)); } 
	   funcTermList                  
	   comprRest
	 ;

comprRest  
	 : RCBRACE			  //{ EndComprHeads(); PushArg(MkCompr()); }
	 | PIPE				  //{ EndComprHeads(); }
	   bodyList 
	   RCBRACE			  //{ PushArg(MkCompr()); }
	 ;

bodyList 
	: body				  //{ AppendBody(); }      
	| body			      //{ AppendBody(); }     
	  SEMICOLON   
	  bodyList
	;

body 
	: constraint          
	| constraint
	  COMMA
	  body
	;

/******************* Terms and Constraints *******************/

constraint
	: funcTerm		      //{ AppendConstraint(MkFind(false, ToSpan(@1))); }

	| id
	  IS
	  funcTerm		      //{ AppendConstraint(MkFind(true, ToSpan(@1))); }

	| NO 
	  compr               //{ AppendConstraint(MkNoConstr(ToSpan(@1))); }

	| NO 
	  funcTerm		      //{ AppendConstraint(MkNoConstr(ToSpan(@1), false)); }

	| NO 
	  id
	  IS
	  funcTerm		      //{ AppendConstraint(MkNoConstr(ToSpan(@1), true)); } 

	| funcTerm
	  relOp
	  funcTerm       //{ AppendConstraint(MkRelConstr()); }
	;

funcTermList
	: funcOrCompr         //{ IncArity(); }
	| funcOrCompr         //{ IncArity(); }
	  COMMA
	  funcTermList
	;

funcOrCompr
	: funcTerm
	| compr				  
	;

funcTerm 
	: atom
		
	| unOp
	  //funcTerm %prec UMINUS //{ PushArg(MkTerm(1)); }
	  funcTerm UMINUS

	| funcTerm				  
	  binOp
	  funcTerm            //{ PushArg(MkTerm(2)); }

	| id				 
	  LPAREN              //{ PushSymbol(); }
	  funcTermList   
	  RPAREN			  //{ PushArg(MkTerm()); }
	  
	| QSTART              //{ PushQuote(ToSpan(@1)); }
	  QuoteList
	  QEND                //{ PushArg(PopQuote());   }
	   
	| LPAREN
	  funcTerm   
	  RPAREN			
	;

quoteList
	 : quoteItem
	 | quoteItem
	   quoteList
	 ;

quoteItem
	 : QUOTERUN    //{ AppendQuoteRun($1.str, ToSpan(@1)); }
	 | QUOTEESCAPE //{ AppendQuoteEscape($1.str, ToSpan(@1)); }
	 | UQSTART
	   funcTerm
	   UQEND	   //{ AppendUnquote(); }
	 ;

atom 
	 : id
	 | constant
	 ;

id
	 : BAREID        //{ PushArg(new Nodes.Id(ToSpan(@1), $1.str));  }
	 | QUALID        //{ PushArg(new Nodes.Id(ToSpan(@1), $1.str));  }
	 ;
	 
constant 
	 : DIGITS      //{ PushArg(ParseNumeric($1.str, false, ToSpan(@1))); }
	 | REAL        //{ PushArg(ParseNumeric($1.str, false, ToSpan(@1))); }
	 | FRAC        //{ PushArg(ParseNumeric($1.str, true,  ToSpan(@1))); }
	 | String      //{ PushArg(GetString()); }
	 ;
	 
unOp 
	 : MINUS       //{ PushSymbol(OpKind.Neg,   ToSpan(@1));  }
	 ;
 
binOp
	   : MUL         //{ PushSymbol(OpKind.Mul,  ToSpan(@1));  }
	   | DIV         //{ PushSymbol(OpKind.Div,  ToSpan(@1));  }
	   | MOD         //{ PushSymbol(OpKind.Mod,  ToSpan(@1));  }
	   | PLUS        //{ PushSymbol(OpKind.Add,  ToSpan(@1));  }
	   | MINUS	     //{ PushSymbol(OpKind.Sub,  ToSpan(@1));  }
	   ;   

relOp : EQ           //{ PushSymbol(RelKind.Eq,  ToSpan(@1));  }
	  | NE           //{ PushSymbol(RelKind.Neq, ToSpan(@1));  }
	  | LT           //{ PushSymbol(RelKind.Lt,  ToSpan(@1));  }
	  | LE           //{ PushSymbol(RelKind.Le,  ToSpan(@1));  }
	  | GT           //{ PushSymbol(RelKind.Gt,  ToSpan(@1));  }
	  | GE           //{ PushSymbol(RelKind.Ge,  ToSpan(@1));  }
	  | COLON        //{ PushSymbol(RelKind.Typ, ToSpan(@1));  }
	  ;

string : strStart
		 strBodyList
		 strEnd
	   | strStart
		 strEnd
	   ;

strStart : STRSNGSTART //{ StartString(ToSpan(@1)); }
         | STRMULSTART //{ StartString(ToSpan(@1)); }
		 ;

strBodyList 
        : strBody
		| strBody
		  strBodyList
		;

strBody : STRSNG     //{ AppendString($1.str); }
	    | STRSNGESC  //{ AppendSingleEscape($1.str); }
        | STRMUL     //{ AppendString($1.str); }
		| STRMULESC  //{ AppendMultiEscape($1.str); }
		;

strEnd  : STRSNGEND //{ EndString(ToSpan(@1)); }
        | STRMULEND //{ EndString(ToSpan(@1)); }
		;

/*
 * Lexer Rules
 */

/* Keywords */
DOMAIN : 'domain' ;
MODEL : 'model' ;
TRANSFORM : 'transform' ;
SYSTEM : 'system' ;

INCLUDES : 'includes' ;
EXTENDS : 'extends' ;
OF : 'of' ;
RETURNS : 'returns' ;
AT : 'at' ;
MACHINE : 'machine' ;

IS : 'is' ;
NO : 'no' ;

NEW : 'new' ;
FUN : 'fun' ;
INJ : 'inj' ;
BIJ : 'bij' ;
SUR : 'sur' ;
ANY : 'any' ;
SUB : 'sub' ;

ENSURES : 'ensures' ;
REQUIRES : 'requires' ;
CONFORMS : 'conforms' ;
SOME : 'some' ;
ATLEAST : 'atleast' ;
ATMOST : 'atmost' ;
PARTIAL : 'partial' ;
INITIALLY: 'initially' ;
NEXT : 'next' ;
PROPERTY : 'property' ;
BOOT : 'boot' ;



CmntStart       
	: [/*] ;
CmntEnd         
	: [*/] ;
CmntStartAlt    
	: [//] ;
LF              
	: [\n\r] ;
NonLF           
	: [^\n\r]* ;

White
	: [ \t\r\n] ;

NonQCntrChars   
	: [^`$\n\r]* ;
NonSMCntrChars  
	: [^\'\"\n\r]* ;


fragment BId
	: [A-Za-z_]([A-Za-z_0-9]*[']*) ;

BID : BID ;

TId   
	: [#]BId([[][0-9]+[\]])? ;

SId             
	: [%]BId([.]BId)* ;

WS
	: [ \t\r\n]+ -> channel(HIDDEN)
	;

DIGITS : [\-+]?[0-9]+ ;
REAL : [\-+]?[0-9]+[.][0-9]+ ;
FRAC : [\-+]?[0-9]+[/][\-+]?[0]*[1-9][0-9]* ;
PIPE : [|] ;
TYPEDEF : '::=' ;
RULE : ':-' ;
RENAMES : '::' ;
RANGE : '..' ;
DOT : '.' ;
COLON : ':' ;
COMMA : ',' ;
SEMICOLON : ';' ;
EQ : '=' ;
NE : '!=' ;
LE : '<=' ;
GE : '>=' ;
LT : '<' ;
GT : '>' ;
PLUS : '+' ;
MINUS : '-' ;
MUL : '*' ;
DIV : [/] ;
MOD : '%' ;
STRONGARROW : '=>' ;
WEAKARROW : '->' ;

LCBRACE : '{' ;
RCBRACE : '}' ;
LSBRACE : '[' ;
RSBRACE : ']' ;
LPAREN : '(' ;
RPAREN : ')' ;
