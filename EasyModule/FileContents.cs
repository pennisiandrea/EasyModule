namespace FilesContents
{
    public static class Module
    {
        public const string MAIN_FILE_NAME = "Main.st";
        public const string MAIN_FILE_CONTENT = 
        """
        PROGRAM _INIT
                
            ResetOutputsAction;
            ResetFeedbacksAction;
                
            MachineState.NextState := __MODNAMECAPITAL___INIT;
            
        END_PROGRAM

        PROGRAM _CYCLIC
            
            // Enable module
            IF NOT g__MODNAME__.Commands.Enable THEN
                ResetOutputsAction;
                ResetFeedbacksAction;		
                RETURN;
            END_IF
            
            // Machine state
            MachineStateManagementAction;
            CASE MachineState.ActualState OF
                
                __MODNAMECAPITAL___INIT:
                    MachineState.NextState := __MODNAMECAPITAL___WAITING;
                    
                __MODNAMECAPITAL___WAITING:
                    MachineState.TimeoutTimer.PT := T#0S; // Timeout disabled in this state
                
                __MODNAMECAPITAL___ERROR:
                    MachineState.TimeoutTimer.PT := T#0S; // Timeout disabled in this state
                    ResetOutputsAction;
                    IF g__MODNAME__.Commands.Reset THEN
                        MachineState.TimeoutState := __MODNAMECAPITAL___INIT;
                    END_IF
                
                ELSE
                    // Throw an alarm/error here
                    MachineState.NextState := __MODNAMECAPITAL___ERROR;			
                
            END_CASE
            
            FeedbackUpdateAction;
            
        END_PROGRAM

        PROGRAM _EXIT
            
        END_PROGRAM
        """;

        public const string ACTIONS_FILE_NAME = "Actions.st";
        public const string ACTIONS_FILE_CONTENT = 
        """
        ACTION FeedbackUpdateAction: 

            g__MODNAME__.Feedbacks.Enabled := TRUE;
            g__MODNAME__.Feedbacks.State := MachineState.ActualState; 

        END_ACTION

        ACTION ResetOutputsAction: 

            memset(ADR(g__MODNAME__.Interface.Outputs),0,SIZEOF(g__MODNAME__.Interface.Outputs));

        END_ACTION

        ACTION ResetFeedbacksAction: 

            memset(ADR(g__MODNAME__.Feedbacks),0,SIZEOF(g__MODNAME__.Feedbacks));

        END_ACTION

        ACTION MachineStateManagementAction: 

            MachineState.NewTriggerState := (MachineState.ActualState <> MachineState.NextState);
            MachineState.TimeoutTimer(IN := MachineState.TimeoutTimer.PT <> T#0S AND NOT MachineState.NewTriggerState);
            IF MachineState.TimeoutTimer.Q THEN
                // Throw an alarm/error here
                MachineState.TimeoutState := MachineState.ActualState;
                MachineState.NextState := __MODNAMECAPITAL___ERROR;		
            END_IF
            
            MachineState.ActualState := MachineState.NextState;

        END_ACTION
        """;

        public const string LOC_TYPES_FILE_NAME = "Types.typ";
        public const string LOC_TYPES_FILE_CONTENT = 
        """
        TYPE
            MachineStateType : 	STRUCT  (*Machine state main type*)
                ActualState : __MODNAME__StateEnum; (*Actual state*)
                NextState : __MODNAME__StateEnum; (*Next state*)
                NewTriggerState : BOOL; (*Trigger state change*)
                TimeoutTimer : TON; (*State timeout*)
                TimeoutState : __MODNAME__StateEnum;
            END_STRUCT;
        END_TYPE
        """;

        public const string LOC_VARIABLES_FILE_NAME = "Variables.var";
        public const string LOC_VARIABLES_FILE_CONTENT = 
        """
        VAR
            MachineState : MachineStateType;
        END_VAR
        """;

        public const string IEC_FILE_NAME = "IEC.prg";
        public const string IEC_FILE_CONTENT = 
        """
        <?xml version="1.0" encoding="utf-8"?>
        <?AutomationStudio FileVersion="4.9"?>
        <Program SubType="IEC" xmlns="http://br-automation.co.at/AS/Program">
        <Files>
            <File Description="Init, cyclic, exit code">Main.st</File>
            <File Description="Local data types" Private="true">Types.typ</File>
            <File Description="Local variables" Private="true">Variables.var</File>
            <File>Actions.st</File>
        </Files>
        </Program>
        """;
        
        public const string ALARMS_TXT_FILE_NAME = "__MODNAME__Alarms.tmx";
        public const string ALARMS_TXT_FILE_CONTENT = 
        """
        <?xml version="1.0" encoding="utf-8"?>
        <tmx version="1.4">
        <header creationtool="B&amp;R Automation Studio" creationtoolversion="4.2" datatype="unknown" segtype="sentence" adminlang="en" srclang="en" o-tmf="TMX">
            <note>Change the namespace to define where this text module should be located within the logical structure of your texts</note>
            <prop type="x-BR-TS:Namespace">Source/__MODNAME__/Alarms</prop>
        </header>
        <body />
        </tmx>
        """;
        
        public const string GLB_TYPES_FILE_NAME = "__MODNAME__GlobalTypes.typ";
        public const string GLB_TYPES_FILE_CONTENT = 
        """
        TYPE
            __MODNAME__Type : 	STRUCT  (*__MODNAME__ Main type*)
                Commands : __MODNAME__CommadsType;
                Feedbacks : __MODNAME__FeedbacksType;
                Parameters : __MODNAME__ParametersType;
                Interface : __MODNAME__InterfaceType;
            END_STRUCT;
            __MODNAME__CommadsType : 	STRUCT  (*__MODNAME__ Commands type*)
                Enable : BOOL;
                Reset : BOOL;
            END_STRUCT;
            __MODNAME__FeedbacksType : 	STRUCT  (*__MODNAME__ Feedbacks type*)
                Enabled : BOOL;
                State : __MODNAME__StateEnum; (*__MODNAME__ actual state*)
            END_STRUCT;
            __MODNAME__ParametersType : 	STRUCT  (*__MODNAME__ Parameters type*)
                Var : BOOL;
            END_STRUCT;
            __MODNAME__InterfaceType : 	STRUCT  (*__MODNAME__ Interface type*)
                Inputs : __MODNAME__InterfaceInputsType;
                Outputs : __MODNAME__InterfaceOutputsType;
            END_STRUCT;
            __MODNAME__InterfaceOutputsType : 	STRUCT  (*__MODNAME__ Interface Output type*)
                Var : BOOL;
            END_STRUCT;
            __MODNAME__InterfaceInputsType : 	STRUCT  (*__MODNAME__ Interface Input type*)
                Var : BOOL;
            END_STRUCT;
            __MODNAME__StateEnum : 
                ( (*__MODNAME__ Machine State enumeration*)
                __MODNAMECAPITAL___INIT, (*__MODNAME__ in INIT state*)
                __MODNAMECAPITAL___WAITING, (*__MODNAME__ in WAITING state*)
                __MODNAMECAPITAL___ERROR (*__MODNAME__ in ERROR state*)
                );
        END_TYPE
        """;
        
        public const string GLB_VARIABLES_FILE_NAME = "__MODNAME__GlobalVariables.var";
        public const string GLB_VARIABLES_FILE_CONTENT = 
        """
        VAR
            g__MODNAME__ : __MODNAME__Type;
        END_VAR
        """;
        
        public const string THIS_PKG_FILE_NAME = "Package.pkg";
        public const string THIS_PKG_FILE_CONTENT = 
        """
        <?xml version="1.0" encoding="utf-8"?>
        <?AutomationStudio FileVersion="4.9"?>
        <Package xmlns="http://br-automation.co.at/AS/Package">
        <Objects>
            <Object Type="File">__MODNAME__GlobalTypes.typ</Object>
            <Object Type="File">__MODNAME__GlobalVariables.var</Object>
            <Object Type="Program" Language="IEC">__MODNAME__Program</Object>
            <Object Type="File">__MODNAME__Alarms.tmx</Object>
        </Objects>
        <Dependencies>
            <Dependency ObjectName="standard" />
            <Dependency ObjectName="asstring" />
        </Dependencies>
        </Package>
        """;
        
        public const string PARENT_PKG_FILE_NAME = "Package.pkg";
        public const string PARENT_PKG_FILE_CONTENT = 
        """
        <?xml version="1.0" encoding="utf-8"?>
        <?AutomationStudio FileVersion="4.9"?>
        <Package xmlns="http://br-automation.co.at/AS/Package">
        <Objects>
            <Object Type="Package">__MODNAME__</Object>
        </Objects>
        </Package>
        """;
    }
}