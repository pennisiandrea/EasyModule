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
            
            //MpRecipeRegParFB(MpLink := ADR(gRecipeXmlMpLink), Enable := TRUE, PVName := ADR('g__MODNAME__.Parameters'));
            
            MachineState.NextState := INIT;
            
        END_PROGRAM

        PROGRAM _CYCLIC
            // Recipe
            //MpRecipeRegParFB(ErrorReset := g__MODNAME__.Commands.Reset AND MpRecipeRegParFB.Error);

            // Alarms
            AlarmsAction;

            // Enable module
            IF NOT g__MODNAME__.Commands.Enable THEN
                ResetOutputsAction;
                ResetFeedbacksAction;		
                RETURN;
            END_IF
            
            // Machine state
            MachineStateManagementAction;
            CASE MachineState.ActualState OF
                
                INIT:
                    MachineState.TimeoutTimer.PT := T#0S; // Timeout disabled in this state
                    MachineState.NextState := WAITING;
                    
                WAITING:
                    MachineState.TimeoutTimer.PT := T#0S; // Timeout disabled in this state
                    IF g__MODNAME__.Commands.Start THEN
                    //    MachineState.NextState := <NEW_STATE>;
                    END_IF 
                
                ERROR:
                    MachineState.TimeoutTimer.PT := T#0S; // Timeout disabled in this state
                    IF MachineState.NewTriggerState THEN
                        ResetOutputsAction;
                    ELSE
                        IF g__MODNAME__.Commands.Reset THEN
                            g__MODNAME__.Commands.Reset := FALSE;
                            MachineState.NextState := MachineState.OldState;
                        END_IF
                    END_IF
                    
                WAITING_STEP_TRIGGER:
                    MachineState.TimeoutTimer.PT := T#0S; // Timeout disabled in this state
                    // Just wait the step trigger. The logic is managed in MachineStateManagementAction action.

                ELSE
                    MachineState.NextState := INIT;			
                
            END_CASE
            
            FeedbacksUpdateAction;
            
        END_PROGRAM

        PROGRAM _EXIT
            //MpRecipeRegParFB(Enable := FALSE);
            
        END_PROGRAM
        """;

        public const string ACTIONS_FILE_NAME = "Actions.st";
        public const string ACTIONS_FILE_CONTENT = 
        """
        ACTION FeedbacksUpdateAction: 

            g__MODNAME__.Feedbacks.Enabled := TRUE;
            g__MODNAME__.Feedbacks.Waiting := MachineState.ActualState = WAITING; 
            g__MODNAME__.Feedbacks.Error := MachineState.ActualState = ERROR;

        END_ACTION

        ACTION ResetOutputsAction: 

            memset(ADR(g__MODNAME__.Interface.Outputs),0,SIZEOF(g__MODNAME__.Interface.Outputs));

        END_ACTION

        ACTION ResetFeedbacksAction: 

            memset(ADR(g__MODNAME__.Feedbacks),0,SIZEOF(g__MODNAME__.Feedbacks));

        END_ACTION

        ACTION MachineStateManagementAction: 

            // Machine state timeout check
            MachineState.TimeoutTimer(IN := MachineState.TimeoutTimer.PT <> T#0S AND NOT MachineState.NewTriggerState);
            IF MachineState.TimeoutTimer.Q THEN
                
                // Throw here timeout alarms
                //CASE MachineState.ActualState OF
                //    <STATE_1_WITH_TIMEOUT>: MpAlarmXSet(gAlarmXCoreMpLink,'<AlarmName>'); // Edge alarm!
                //    <STATE_2_WITH_TIMEOUT>: MpAlarmXSet(gAlarmXCoreMpLink,'<AlarmName>'); // Edge alarm!
                //END_CASE

                MachineState.NextState := ERROR;		
            END_IF            
            
            // Machine state change state logic
            MachineState.NewTriggerState := (MachineState.ActualState <> MachineState.NextState);
            IF MachineState.NewTriggerState AND MachineState.ActualState <> WAITING_STEP_TRIGGER THEN
                MachineState.OldState := MachineState.ActualState;
                
                IF MachineState.StepByStepEnable THEN
                    MachineState.ActualState := WAITING_STEP_TRIGGER;
                END_IF
            END_IF
            IF NOT MachineState.StepByStepEnable OR MachineState.StepByStepTrigger THEN
                MachineState.ActualState := MachineState.NextState;
                MachineState.StepByStepTrigger := FALSE;	
            END_IF

        END_ACTION
        
        ACTION AlarmsAction:             
            // Throw here alarms which should be checked continuously
            // Machine state timeout alarms are managed in MachineStateManagementAction

            // IF <Condition> AND g__MODNAME__.Commands.Enable THEN
            //      MpAlarmXSet(gAlarmXCoreMpLink,'<AlarmName>');
            //      MachineState.NextState := ERROR;
            // ELSE
            //      MpAlarmXReset(gAlarmXCoreMpLink,'<AlarmName>'); // Reset only for persistent alarms
            // END_IF

        END_ACTION
        """;

        public const string LOC_TYPES_FILE_NAME = "Types.typ";
        public const string LOC_TYPES_FILE_CONTENT = 
        """
        TYPE
            MachineStateType : 	STRUCT  (*Machine state main type*)
                OldState : MachineStateEnum; (*Actual state*)
                ActualState : MachineStateEnum; (*Actual state*)
                NextState : MachineStateEnum; (*Next state*)
                NewTriggerState : BOOL; (*Trigger state change*)
                TimeoutTimer : TON; (*State timeout*)
                StepByStepEnable : BOOL; (*Enable of Step by Step mode*)
                StepByStepTrigger : BOOL; (*Trigger to change step when Step by Step mode is active*)
            END_STRUCT;
            MachineStateEnum : 
                ( (*Machine State enumeration*)
                INIT, (*INIT state*)
                WAITING, (*WAITING state*)
                ERROR, (*ERROR state*)
                WAITING_STEP_TRIGGER (*WAITING trigger in StepByStep mode*)
                );
        END_TYPE
        """;

        public const string LOC_VARIABLES_FILE_NAME = "Variables.var";
        public const string LOC_VARIABLES_FILE_CONTENT = 
        """
        VAR
            MachineState : MachineStateType;
            MpRecipeRegParFB : MpRecipeRegPar; (*Recipe register functionblock*)
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
                Start : BOOL;
                Reset : BOOL;
            END_STRUCT;
            __MODNAME__FeedbacksType : 	STRUCT  (*__MODNAME__ Feedbacks type*)
                Enabled : BOOL;
                Waiting : BOOL;
                Error   : BOOL;
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