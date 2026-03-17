#define ICALL_TABLE_corlib 1

static int corlib_icall_indexes [] = {
243,
255,
256,
257,
258,
259,
260,
261,
262,
263,
266,
267,
268,
442,
443,
444,
473,
474,
475,
495,
496,
497,
498,
615,
616,
617,
620,
674,
675,
676,
679,
681,
683,
685,
690,
698,
699,
700,
701,
702,
703,
704,
705,
706,
707,
708,
709,
710,
711,
712,
713,
714,
716,
717,
718,
719,
720,
721,
722,
814,
815,
816,
817,
818,
819,
820,
821,
822,
823,
824,
825,
826,
827,
828,
829,
830,
832,
833,
834,
835,
836,
837,
838,
905,
906,
975,
982,
985,
987,
992,
993,
995,
996,
1000,
1002,
1003,
1005,
1007,
1008,
1011,
1012,
1013,
1016,
1018,
1021,
1023,
1025,
1034,
1102,
1104,
1106,
1116,
1117,
1118,
1119,
1121,
1128,
1129,
1130,
1131,
1132,
1140,
1141,
1142,
1146,
1147,
1149,
1153,
1154,
1155,
1439,
1632,
1633,
9845,
9846,
9848,
9849,
9850,
9851,
9852,
9853,
9855,
9857,
9859,
9860,
9861,
9874,
9876,
9884,
9886,
9888,
9890,
9943,
9949,
9950,
9952,
9953,
9954,
9955,
9956,
9958,
9960,
9961,
11260,
11264,
11266,
11267,
11268,
11269,
11534,
11535,
11536,
11537,
11557,
11558,
11559,
11561,
11637,
11729,
11731,
11733,
11743,
11744,
11745,
11746,
11747,
12227,
12228,
12229,
12234,
12235,
12314,
12315,
12341,
12348,
12355,
12366,
12369,
12394,
12471,
12484,
12486,
12488,
12511,
12513,
12514,
12515,
12516,
12517,
12526,
12541,
12561,
12562,
12570,
12572,
12579,
12580,
12583,
12585,
12590,
12596,
12597,
12604,
12606,
12618,
12621,
12622,
12623,
12634,
12643,
12649,
12650,
12651,
12653,
12654,
12671,
12673,
12687,
12710,
12711,
12712,
12737,
12742,
12772,
12773,
13343,
13357,
13458,
13459,
13684,
13685,
13694,
13695,
13696,
13702,
13799,
14395,
14396,
15011,
15012,
15013,
15018,
15028,
16075,
16096,
16098,
16100,
};
void ves_icall_System_Array_InternalCreate (int,int,int,int,int);
int ves_icall_System_Array_GetCorElementTypeOfElementTypeInternal (int);
int ves_icall_System_Array_IsValueOfElementTypeInternal (int,int);
int ves_icall_System_Array_CanChangePrimitive (int,int,int);
int ves_icall_System_Array_FastCopy (int,int,int,int,int);
int ves_icall_System_Array_GetLengthInternal_raw (int,int,int);
int ves_icall_System_Array_GetLowerBoundInternal_raw (int,int,int);
void ves_icall_System_Array_GetGenericValue_icall (int,int,int);
void ves_icall_System_Array_GetValueImpl_raw (int,int,int,int);
void ves_icall_System_Array_SetGenericValue_icall (int,int,int);
void ves_icall_System_Array_SetValueImpl_raw (int,int,int,int);
void ves_icall_System_Array_InitializeInternal_raw (int,int);
void ves_icall_System_Array_SetValueRelaxedImpl_raw (int,int,int,int);
void ves_icall_System_Runtime_RuntimeImports_ZeroMemory (int,int);
void ves_icall_System_Runtime_RuntimeImports_Memmove (int,int,int);
void ves_icall_System_Buffer_BulkMoveWithWriteBarrier (int,int,int,int);
int ves_icall_System_Delegate_AllocDelegateLike_internal_raw (int,int);
int ves_icall_System_Delegate_CreateDelegate_internal_raw (int,int,int,int,int);
int ves_icall_System_Delegate_GetVirtualMethod_internal_raw (int,int);
void ves_icall_System_Enum_GetEnumValuesAndNames_raw (int,int,int,int);
void ves_icall_System_Enum_InternalBoxEnum_raw (int,int,int64_t,int);
int ves_icall_System_Enum_InternalGetCorElementType (int);
void ves_icall_System_Enum_InternalGetUnderlyingType_raw (int,int,int);
int ves_icall_System_Environment_get_ProcessorCount ();
int ves_icall_System_Environment_get_TickCount ();
int64_t ves_icall_System_Environment_get_TickCount64 ();
void ves_icall_System_Environment_FailFast_raw (int,int,int,int);
int ves_icall_System_GC_GetCollectionCount (int);
void ves_icall_System_GC_register_ephemeron_array_raw (int,int);
int ves_icall_System_GC_get_ephemeron_tombstone_raw (int);
void ves_icall_System_GC_SuppressFinalize_raw (int,int);
void ves_icall_System_GC_ReRegisterForFinalize_raw (int,int);
void ves_icall_System_GC_GetGCMemoryInfo (int,int,int,int,int,int);
int ves_icall_System_GC_AllocPinnedArray_raw (int,int,int);
int ves_icall_System_Object_MemberwiseClone_raw (int,int);
double ves_icall_System_Math_Acos (double);
double ves_icall_System_Math_Acosh (double);
double ves_icall_System_Math_Asin (double);
double ves_icall_System_Math_Asinh (double);
double ves_icall_System_Math_Atan (double);
double ves_icall_System_Math_Atan2 (double,double);
double ves_icall_System_Math_Atanh (double);
double ves_icall_System_Math_Cbrt (double);
double ves_icall_System_Math_Ceiling (double);
double ves_icall_System_Math_Cos (double);
double ves_icall_System_Math_Cosh (double);
double ves_icall_System_Math_Exp (double);
double ves_icall_System_Math_Floor (double);
double ves_icall_System_Math_Log (double);
double ves_icall_System_Math_Log10 (double);
double ves_icall_System_Math_Pow (double,double);
double ves_icall_System_Math_Sin (double);
double ves_icall_System_Math_Sinh (double);
double ves_icall_System_Math_Sqrt (double);
double ves_icall_System_Math_Tan (double);
double ves_icall_System_Math_Tanh (double);
double ves_icall_System_Math_FusedMultiplyAdd (double,double,double);
double ves_icall_System_Math_Log2 (double);
double ves_icall_System_Math_ModF (double,int);
float ves_icall_System_MathF_Acos (float);
float ves_icall_System_MathF_Acosh (float);
float ves_icall_System_MathF_Asin (float);
float ves_icall_System_MathF_Asinh (float);
float ves_icall_System_MathF_Atan (float);
float ves_icall_System_MathF_Atan2 (float,float);
float ves_icall_System_MathF_Atanh (float);
float ves_icall_System_MathF_Cbrt (float);
float ves_icall_System_MathF_Ceiling (float);
float ves_icall_System_MathF_Cos (float);
float ves_icall_System_MathF_Cosh (float);
float ves_icall_System_MathF_Exp (float);
float ves_icall_System_MathF_Floor (float);
float ves_icall_System_MathF_Log (float);
float ves_icall_System_MathF_Log10 (float);
float ves_icall_System_MathF_Pow (float,float);
float ves_icall_System_MathF_Sin (float);
float ves_icall_System_MathF_Sinh (float);
float ves_icall_System_MathF_Sqrt (float);
float ves_icall_System_MathF_Tan (float);
float ves_icall_System_MathF_Tanh (float);
float ves_icall_System_MathF_FusedMultiplyAdd (float,float,float);
float ves_icall_System_MathF_Log2 (float);
float ves_icall_System_MathF_ModF (float,int);
void ves_icall_RuntimeMethodHandle_ReboxFromNullable_raw (int,int,int);
void ves_icall_RuntimeMethodHandle_ReboxToNullable_raw (int,int,int,int);
int ves_icall_RuntimeType_GetCorrespondingInflatedMethod_raw (int,int,int);
void ves_icall_RuntimeType_make_array_type_raw (int,int,int,int);
void ves_icall_RuntimeType_make_byref_type_raw (int,int,int);
void ves_icall_RuntimeType_make_pointer_type_raw (int,int,int);
void ves_icall_RuntimeType_MakeGenericType_raw (int,int,int,int);
int ves_icall_RuntimeType_GetMethodsByName_native_raw (int,int,int,int,int);
int ves_icall_RuntimeType_GetPropertiesByName_native_raw (int,int,int,int,int);
int ves_icall_RuntimeType_GetConstructors_native_raw (int,int,int);
void ves_icall_RuntimeType_GetInterfaceMapData_raw (int,int,int,int,int);
int ves_icall_System_RuntimeType_CreateInstanceInternal_raw (int,int);
void ves_icall_System_RuntimeType_AllocateValueType_raw (int,int,int,int);
void ves_icall_RuntimeType_GetDeclaringMethod_raw (int,int,int);
void ves_icall_System_RuntimeType_getFullName_raw (int,int,int,int,int);
void ves_icall_RuntimeType_GetGenericArgumentsInternal_raw (int,int,int,int);
int ves_icall_RuntimeType_GetGenericParameterPosition (int);
int ves_icall_RuntimeType_GetEvents_native_raw (int,int,int,int);
int ves_icall_RuntimeType_GetFields_native_raw (int,int,int,int,int);
void ves_icall_RuntimeType_GetInterfaces_raw (int,int,int);
int ves_icall_RuntimeType_GetNestedTypes_native_raw (int,int,int,int,int);
void ves_icall_RuntimeType_GetDeclaringType_raw (int,int,int);
void ves_icall_RuntimeType_GetName_raw (int,int,int);
void ves_icall_RuntimeType_GetNamespace_raw (int,int,int);
int ves_icall_RuntimeType_FunctionPointerReturnAndParameterTypes_raw (int,int);
int ves_icall_RuntimeTypeHandle_GetAttributes (int);
int ves_icall_RuntimeTypeHandle_GetMetadataToken_raw (int,int);
void ves_icall_RuntimeTypeHandle_GetGenericTypeDefinition_impl_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_GetCorElementType (int);
int ves_icall_RuntimeTypeHandle_HasInstantiation (int);
int ves_icall_RuntimeTypeHandle_IsComObject_raw (int,int);
int ves_icall_RuntimeTypeHandle_IsInstanceOfType_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_HasReferences_raw (int,int);
int ves_icall_RuntimeTypeHandle_GetArrayRank_raw (int,int);
void ves_icall_RuntimeTypeHandle_GetAssembly_raw (int,int,int);
void ves_icall_RuntimeTypeHandle_GetElementType_raw (int,int,int);
void ves_icall_RuntimeTypeHandle_GetModule_raw (int,int,int);
void ves_icall_RuntimeTypeHandle_GetBaseType_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_type_is_assignable_from_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_IsGenericTypeDefinition (int);
int ves_icall_RuntimeTypeHandle_GetGenericParameterInfo_raw (int,int);
int ves_icall_RuntimeTypeHandle_is_subclass_of_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_IsByRefLike_raw (int,int);
void ves_icall_System_RuntimeTypeHandle_internal_from_name_raw (int,int,int,int,int,int);
int ves_icall_System_String_FastAllocateString_raw (int,int);
int ves_icall_System_String_InternalIsInterned_raw (int,int);
int ves_icall_System_String_InternalIntern_raw (int,int);
int ves_icall_System_Type_internal_from_handle_raw (int,int);
int ves_icall_System_ValueType_InternalGetHashCode_raw (int,int,int);
int ves_icall_System_ValueType_Equals_raw (int,int,int,int);
int ves_icall_System_Threading_Interlocked_CompareExchange_Int (int,int,int);
void ves_icall_System_Threading_Interlocked_CompareExchange_Object (int,int,int,int);
int ves_icall_System_Threading_Interlocked_Decrement_Int (int);
int64_t ves_icall_System_Threading_Interlocked_Decrement_Long (int);
int ves_icall_System_Threading_Interlocked_Increment_Int (int);
int64_t ves_icall_System_Threading_Interlocked_Increment_Long (int);
int ves_icall_System_Threading_Interlocked_Exchange_Int (int,int);
void ves_icall_System_Threading_Interlocked_Exchange_Object (int,int,int);
int64_t ves_icall_System_Threading_Interlocked_CompareExchange_Long (int,int64_t,int64_t);
int64_t ves_icall_System_Threading_Interlocked_Exchange_Long (int,int64_t);
int64_t ves_icall_System_Threading_Interlocked_Read_Long (int);
int ves_icall_System_Threading_Interlocked_Add_Int (int,int);
int64_t ves_icall_System_Threading_Interlocked_Add_Long (int,int64_t);
void ves_icall_System_Threading_Monitor_Monitor_Enter_raw (int,int);
void mono_monitor_exit_icall_raw (int,int);
void ves_icall_System_Threading_Monitor_Monitor_pulse_raw (int,int);
void ves_icall_System_Threading_Monitor_Monitor_pulse_all_raw (int,int);
int ves_icall_System_Threading_Monitor_Monitor_wait_raw (int,int,int,int);
void ves_icall_System_Threading_Monitor_Monitor_try_enter_with_atomic_var_raw (int,int,int,int,int);
void ves_icall_System_Threading_Thread_StartInternal_raw (int,int,int);
void ves_icall_System_Threading_Thread_InitInternal_raw (int,int);
int ves_icall_System_Threading_Thread_GetCurrentThread ();
void ves_icall_System_Threading_InternalThread_Thread_free_internal_raw (int,int);
int ves_icall_System_Threading_Thread_GetState_raw (int,int);
void ves_icall_System_Threading_Thread_SetState_raw (int,int,int);
void ves_icall_System_Threading_Thread_ClrState_raw (int,int,int);
void ves_icall_System_Threading_Thread_SetName_icall_raw (int,int,int,int);
int ves_icall_System_Threading_Thread_YieldInternal ();
int ves_icall_System_Threading_Thread_Join_internal_raw (int,int,int);
void ves_icall_System_Threading_Thread_SetPriority_raw (int,int,int);
void ves_icall_System_Runtime_Loader_AssemblyLoadContext_PrepareForAssemblyLoadContextRelease_raw (int,int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_GetLoadContextForAssembly_raw (int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalLoadFile_raw (int,int,int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalInitializeNativeALC_raw (int,int,int,int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalLoadFromStream_raw (int,int,int,int,int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalGetLoadedAssemblies_raw (int);
int ves_icall_System_GCHandle_InternalAlloc_raw (int,int,int);
void ves_icall_System_GCHandle_InternalFree_raw (int,int);
int ves_icall_System_GCHandle_InternalGet_raw (int,int);
void ves_icall_System_GCHandle_InternalSet_raw (int,int,int);
int ves_icall_System_Runtime_InteropServices_Marshal_GetLastPInvokeError ();
void ves_icall_System_Runtime_InteropServices_Marshal_SetLastPInvokeError (int);
void ves_icall_System_Runtime_InteropServices_Marshal_StructureToPtr_raw (int,int,int,int);
int ves_icall_System_Runtime_InteropServices_Marshal_SizeOfHelper_raw (int,int,int);
int ves_icall_System_Runtime_InteropServices_NativeLibrary_LoadByName_raw (int,int,int,int,int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InternalGetHashCode_raw (int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InternalTryGetHashCode_raw (int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetObjectValue_raw (int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetUninitializedObjectInternal_raw (int,int);
void ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray_raw (int,int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetSpanDataFrom_raw (int,int,int,int);
void ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_RunClassConstructor_raw (int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_SufficientExecutionStack ();
int ves_icall_System_Reflection_Assembly_GetExecutingAssembly_raw (int,int);
int ves_icall_System_Reflection_Assembly_GetCallingAssembly_raw (int);
int ves_icall_System_Reflection_Assembly_GetEntryAssembly_raw (int);
int ves_icall_System_Reflection_Assembly_InternalLoad_raw (int,int,int,int);
int ves_icall_System_Reflection_Assembly_InternalGetType_raw (int,int,int,int,int,int);
void ves_icall_System_Reflection_AssemblyName_FreeAssemblyName (int,int);
int ves_icall_System_Reflection_AssemblyName_GetNativeName (int);
int ves_icall_MonoCustomAttrs_GetCustomAttributesInternal_raw (int,int,int,int);
int ves_icall_MonoCustomAttrs_GetCustomAttributesDataInternal_raw (int,int);
int ves_icall_MonoCustomAttrs_IsDefinedInternal_raw (int,int,int);
int ves_icall_System_Reflection_FieldInfo_internal_from_handle_type_raw (int,int,int);
int ves_icall_System_Reflection_FieldInfo_get_marshal_info_raw (int,int);
int ves_icall_System_Reflection_LoaderAllocatorScout_Destroy (int);
void ves_icall_System_Reflection_RuntimeAssembly_GetEntryPoint_raw (int,int,int);
void ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceNames_raw (int,int,int);
void ves_icall_System_Reflection_RuntimeAssembly_GetExportedTypes_raw (int,int,int);
void ves_icall_System_Reflection_RuntimeAssembly_GetTopLevelForwardedTypes_raw (int,int,int);
void ves_icall_System_Reflection_RuntimeAssembly_GetInfo_raw (int,int,int,int);
int ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceInfoInternal_raw (int,int,int,int);
int ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceInternal_raw (int,int,int,int,int);
void ves_icall_System_Reflection_Assembly_GetManifestModuleInternal_raw (int,int,int);
void ves_icall_System_Reflection_RuntimeAssembly_GetModulesInternal_raw (int,int,int);
int ves_icall_System_Reflection_Assembly_InternalGetReferencedAssemblies_raw (int,int);
void ves_icall_System_Reflection_RuntimeCustomAttributeData_ResolveArgumentsInternal_raw (int,int,int,int,int,int,int);
void ves_icall_RuntimeEventInfo_get_event_info_raw (int,int,int);
int ves_icall_reflection_get_token_raw (int,int);
int ves_icall_System_Reflection_EventInfo_internal_from_handle_type_raw (int,int,int);
int ves_icall_RuntimeFieldInfo_ResolveType_raw (int,int);
int ves_icall_RuntimeFieldInfo_GetParentType_raw (int,int,int);
int ves_icall_RuntimeFieldInfo_GetFieldOffset_raw (int,int);
int ves_icall_RuntimeFieldInfo_GetValueInternal_raw (int,int,int);
void ves_icall_RuntimeFieldInfo_SetValueInternal_raw (int,int,int,int);
int ves_icall_RuntimeFieldInfo_GetRawConstantValue_raw (int,int);
int ves_icall_reflection_get_token_raw (int,int);
void ves_icall_get_method_info_raw (int,int,int);
int ves_icall_get_method_attributes (int);
int ves_icall_System_Reflection_MonoMethodInfo_get_parameter_info_raw (int,int,int);
int ves_icall_System_MonoMethodInfo_get_retval_marshal_raw (int,int);
int ves_icall_System_Reflection_RuntimeMethodInfo_GetMethodFromHandleInternalType_native_raw (int,int,int,int);
int ves_icall_RuntimeMethodInfo_get_name_raw (int,int);
int ves_icall_RuntimeMethodInfo_get_base_method_raw (int,int,int);
int ves_icall_reflection_get_token_raw (int,int);
int ves_icall_InternalInvoke_raw (int,int,int,int,int);
void ves_icall_RuntimeMethodInfo_GetPInvoke_raw (int,int,int,int,int);
int ves_icall_RuntimeMethodInfo_MakeGenericMethod_impl_raw (int,int,int);
int ves_icall_RuntimeMethodInfo_GetGenericArguments_raw (int,int);
int ves_icall_RuntimeMethodInfo_GetGenericMethodDefinition_raw (int,int);
int ves_icall_RuntimeMethodInfo_get_IsGenericMethodDefinition_raw (int,int);
int ves_icall_RuntimeMethodInfo_get_IsGenericMethod_raw (int,int);
void ves_icall_InvokeClassConstructor_raw (int,int);
int ves_icall_InternalInvoke_raw (int,int,int,int,int);
int ves_icall_reflection_get_token_raw (int,int);
int ves_icall_System_Reflection_RuntimeModule_InternalGetTypes_raw (int,int);
void ves_icall_System_Reflection_RuntimeModule_GetGuidInternal_raw (int,int,int);
int ves_icall_System_Reflection_RuntimeModule_ResolveMethodToken_raw (int,int,int,int,int,int);
int ves_icall_RuntimeParameterInfo_GetTypeModifiers_raw (int,int,int,int,int,int);
void ves_icall_RuntimePropertyInfo_get_property_info_raw (int,int,int,int);
int ves_icall_reflection_get_token_raw (int,int);
int ves_icall_System_Reflection_RuntimePropertyInfo_internal_from_handle_type_raw (int,int,int);
int ves_icall_CustomAttributeBuilder_GetBlob_raw (int,int,int,int,int,int,int,int);
void ves_icall_DynamicMethod_create_dynamic_method_raw (int,int,int,int,int);
void ves_icall_AssemblyBuilder_basic_init_raw (int,int);
void ves_icall_AssemblyBuilder_UpdateNativeCustomAttributes_raw (int,int);
void ves_icall_ModuleBuilder_basic_init_raw (int,int);
void ves_icall_ModuleBuilder_set_wrappers_type_raw (int,int,int);
int ves_icall_ModuleBuilder_getUSIndex_raw (int,int,int);
int ves_icall_ModuleBuilder_getToken_raw (int,int,int,int);
int ves_icall_ModuleBuilder_getMethodToken_raw (int,int,int,int);
void ves_icall_ModuleBuilder_RegisterToken_raw (int,int,int,int);
int ves_icall_TypeBuilder_create_runtime_class_raw (int,int);
int ves_icall_System_IO_Stream_HasOverriddenBeginEndRead_raw (int,int);
int ves_icall_System_IO_Stream_HasOverriddenBeginEndWrite_raw (int,int);
int ves_icall_System_Diagnostics_Debugger_IsAttached_internal ();
int ves_icall_System_Diagnostics_Debugger_IsLogging ();
void ves_icall_System_Diagnostics_Debugger_Log (int,int,int);
int ves_icall_System_Diagnostics_StackFrame_GetFrameInfo (int,int,int,int,int,int,int,int);
void ves_icall_System_Diagnostics_StackTrace_GetTrace (int,int,int,int);
int ves_icall_Mono_RuntimeClassHandle_GetTypeFromClass (int);
void ves_icall_Mono_RuntimeGPtrArrayHandle_GPtrArrayFree (int);
int ves_icall_Mono_SafeStringMarshal_StringToUtf8 (int);
void ves_icall_Mono_SafeStringMarshal_GFree (int);
static void *corlib_icall_funcs [] = {
// token 243,
ves_icall_System_Array_InternalCreate,
// token 255,
ves_icall_System_Array_GetCorElementTypeOfElementTypeInternal,
// token 256,
ves_icall_System_Array_IsValueOfElementTypeInternal,
// token 257,
ves_icall_System_Array_CanChangePrimitive,
// token 258,
ves_icall_System_Array_FastCopy,
// token 259,
ves_icall_System_Array_GetLengthInternal_raw,
// token 260,
ves_icall_System_Array_GetLowerBoundInternal_raw,
// token 261,
ves_icall_System_Array_GetGenericValue_icall,
// token 262,
ves_icall_System_Array_GetValueImpl_raw,
// token 263,
ves_icall_System_Array_SetGenericValue_icall,
// token 266,
ves_icall_System_Array_SetValueImpl_raw,
// token 267,
ves_icall_System_Array_InitializeInternal_raw,
// token 268,
ves_icall_System_Array_SetValueRelaxedImpl_raw,
// token 442,
ves_icall_System_Runtime_RuntimeImports_ZeroMemory,
// token 443,
ves_icall_System_Runtime_RuntimeImports_Memmove,
// token 444,
ves_icall_System_Buffer_BulkMoveWithWriteBarrier,
// token 473,
ves_icall_System_Delegate_AllocDelegateLike_internal_raw,
// token 474,
ves_icall_System_Delegate_CreateDelegate_internal_raw,
// token 475,
ves_icall_System_Delegate_GetVirtualMethod_internal_raw,
// token 495,
ves_icall_System_Enum_GetEnumValuesAndNames_raw,
// token 496,
ves_icall_System_Enum_InternalBoxEnum_raw,
// token 497,
ves_icall_System_Enum_InternalGetCorElementType,
// token 498,
ves_icall_System_Enum_InternalGetUnderlyingType_raw,
// token 615,
ves_icall_System_Environment_get_ProcessorCount,
// token 616,
ves_icall_System_Environment_get_TickCount,
// token 617,
ves_icall_System_Environment_get_TickCount64,
// token 620,
ves_icall_System_Environment_FailFast_raw,
// token 674,
ves_icall_System_GC_GetCollectionCount,
// token 675,
ves_icall_System_GC_register_ephemeron_array_raw,
// token 676,
ves_icall_System_GC_get_ephemeron_tombstone_raw,
// token 679,
ves_icall_System_GC_SuppressFinalize_raw,
// token 681,
ves_icall_System_GC_ReRegisterForFinalize_raw,
// token 683,
ves_icall_System_GC_GetGCMemoryInfo,
// token 685,
ves_icall_System_GC_AllocPinnedArray_raw,
// token 690,
ves_icall_System_Object_MemberwiseClone_raw,
// token 698,
ves_icall_System_Math_Acos,
// token 699,
ves_icall_System_Math_Acosh,
// token 700,
ves_icall_System_Math_Asin,
// token 701,
ves_icall_System_Math_Asinh,
// token 702,
ves_icall_System_Math_Atan,
// token 703,
ves_icall_System_Math_Atan2,
// token 704,
ves_icall_System_Math_Atanh,
// token 705,
ves_icall_System_Math_Cbrt,
// token 706,
ves_icall_System_Math_Ceiling,
// token 707,
ves_icall_System_Math_Cos,
// token 708,
ves_icall_System_Math_Cosh,
// token 709,
ves_icall_System_Math_Exp,
// token 710,
ves_icall_System_Math_Floor,
// token 711,
ves_icall_System_Math_Log,
// token 712,
ves_icall_System_Math_Log10,
// token 713,
ves_icall_System_Math_Pow,
// token 714,
ves_icall_System_Math_Sin,
// token 716,
ves_icall_System_Math_Sinh,
// token 717,
ves_icall_System_Math_Sqrt,
// token 718,
ves_icall_System_Math_Tan,
// token 719,
ves_icall_System_Math_Tanh,
// token 720,
ves_icall_System_Math_FusedMultiplyAdd,
// token 721,
ves_icall_System_Math_Log2,
// token 722,
ves_icall_System_Math_ModF,
// token 814,
ves_icall_System_MathF_Acos,
// token 815,
ves_icall_System_MathF_Acosh,
// token 816,
ves_icall_System_MathF_Asin,
// token 817,
ves_icall_System_MathF_Asinh,
// token 818,
ves_icall_System_MathF_Atan,
// token 819,
ves_icall_System_MathF_Atan2,
// token 820,
ves_icall_System_MathF_Atanh,
// token 821,
ves_icall_System_MathF_Cbrt,
// token 822,
ves_icall_System_MathF_Ceiling,
// token 823,
ves_icall_System_MathF_Cos,
// token 824,
ves_icall_System_MathF_Cosh,
// token 825,
ves_icall_System_MathF_Exp,
// token 826,
ves_icall_System_MathF_Floor,
// token 827,
ves_icall_System_MathF_Log,
// token 828,
ves_icall_System_MathF_Log10,
// token 829,
ves_icall_System_MathF_Pow,
// token 830,
ves_icall_System_MathF_Sin,
// token 832,
ves_icall_System_MathF_Sinh,
// token 833,
ves_icall_System_MathF_Sqrt,
// token 834,
ves_icall_System_MathF_Tan,
// token 835,
ves_icall_System_MathF_Tanh,
// token 836,
ves_icall_System_MathF_FusedMultiplyAdd,
// token 837,
ves_icall_System_MathF_Log2,
// token 838,
ves_icall_System_MathF_ModF,
// token 905,
ves_icall_RuntimeMethodHandle_ReboxFromNullable_raw,
// token 906,
ves_icall_RuntimeMethodHandle_ReboxToNullable_raw,
// token 975,
ves_icall_RuntimeType_GetCorrespondingInflatedMethod_raw,
// token 982,
ves_icall_RuntimeType_make_array_type_raw,
// token 985,
ves_icall_RuntimeType_make_byref_type_raw,
// token 987,
ves_icall_RuntimeType_make_pointer_type_raw,
// token 992,
ves_icall_RuntimeType_MakeGenericType_raw,
// token 993,
ves_icall_RuntimeType_GetMethodsByName_native_raw,
// token 995,
ves_icall_RuntimeType_GetPropertiesByName_native_raw,
// token 996,
ves_icall_RuntimeType_GetConstructors_native_raw,
// token 1000,
ves_icall_RuntimeType_GetInterfaceMapData_raw,
// token 1002,
ves_icall_System_RuntimeType_CreateInstanceInternal_raw,
// token 1003,
ves_icall_System_RuntimeType_AllocateValueType_raw,
// token 1005,
ves_icall_RuntimeType_GetDeclaringMethod_raw,
// token 1007,
ves_icall_System_RuntimeType_getFullName_raw,
// token 1008,
ves_icall_RuntimeType_GetGenericArgumentsInternal_raw,
// token 1011,
ves_icall_RuntimeType_GetGenericParameterPosition,
// token 1012,
ves_icall_RuntimeType_GetEvents_native_raw,
// token 1013,
ves_icall_RuntimeType_GetFields_native_raw,
// token 1016,
ves_icall_RuntimeType_GetInterfaces_raw,
// token 1018,
ves_icall_RuntimeType_GetNestedTypes_native_raw,
// token 1021,
ves_icall_RuntimeType_GetDeclaringType_raw,
// token 1023,
ves_icall_RuntimeType_GetName_raw,
// token 1025,
ves_icall_RuntimeType_GetNamespace_raw,
// token 1034,
ves_icall_RuntimeType_FunctionPointerReturnAndParameterTypes_raw,
// token 1102,
ves_icall_RuntimeTypeHandle_GetAttributes,
// token 1104,
ves_icall_RuntimeTypeHandle_GetMetadataToken_raw,
// token 1106,
ves_icall_RuntimeTypeHandle_GetGenericTypeDefinition_impl_raw,
// token 1116,
ves_icall_RuntimeTypeHandle_GetCorElementType,
// token 1117,
ves_icall_RuntimeTypeHandle_HasInstantiation,
// token 1118,
ves_icall_RuntimeTypeHandle_IsComObject_raw,
// token 1119,
ves_icall_RuntimeTypeHandle_IsInstanceOfType_raw,
// token 1121,
ves_icall_RuntimeTypeHandle_HasReferences_raw,
// token 1128,
ves_icall_RuntimeTypeHandle_GetArrayRank_raw,
// token 1129,
ves_icall_RuntimeTypeHandle_GetAssembly_raw,
// token 1130,
ves_icall_RuntimeTypeHandle_GetElementType_raw,
// token 1131,
ves_icall_RuntimeTypeHandle_GetModule_raw,
// token 1132,
ves_icall_RuntimeTypeHandle_GetBaseType_raw,
// token 1140,
ves_icall_RuntimeTypeHandle_type_is_assignable_from_raw,
// token 1141,
ves_icall_RuntimeTypeHandle_IsGenericTypeDefinition,
// token 1142,
ves_icall_RuntimeTypeHandle_GetGenericParameterInfo_raw,
// token 1146,
ves_icall_RuntimeTypeHandle_is_subclass_of_raw,
// token 1147,
ves_icall_RuntimeTypeHandle_IsByRefLike_raw,
// token 1149,
ves_icall_System_RuntimeTypeHandle_internal_from_name_raw,
// token 1153,
ves_icall_System_String_FastAllocateString_raw,
// token 1154,
ves_icall_System_String_InternalIsInterned_raw,
// token 1155,
ves_icall_System_String_InternalIntern_raw,
// token 1439,
ves_icall_System_Type_internal_from_handle_raw,
// token 1632,
ves_icall_System_ValueType_InternalGetHashCode_raw,
// token 1633,
ves_icall_System_ValueType_Equals_raw,
// token 9845,
ves_icall_System_Threading_Interlocked_CompareExchange_Int,
// token 9846,
ves_icall_System_Threading_Interlocked_CompareExchange_Object,
// token 9848,
ves_icall_System_Threading_Interlocked_Decrement_Int,
// token 9849,
ves_icall_System_Threading_Interlocked_Decrement_Long,
// token 9850,
ves_icall_System_Threading_Interlocked_Increment_Int,
// token 9851,
ves_icall_System_Threading_Interlocked_Increment_Long,
// token 9852,
ves_icall_System_Threading_Interlocked_Exchange_Int,
// token 9853,
ves_icall_System_Threading_Interlocked_Exchange_Object,
// token 9855,
ves_icall_System_Threading_Interlocked_CompareExchange_Long,
// token 9857,
ves_icall_System_Threading_Interlocked_Exchange_Long,
// token 9859,
ves_icall_System_Threading_Interlocked_Read_Long,
// token 9860,
ves_icall_System_Threading_Interlocked_Add_Int,
// token 9861,
ves_icall_System_Threading_Interlocked_Add_Long,
// token 9874,
ves_icall_System_Threading_Monitor_Monitor_Enter_raw,
// token 9876,
mono_monitor_exit_icall_raw,
// token 9884,
ves_icall_System_Threading_Monitor_Monitor_pulse_raw,
// token 9886,
ves_icall_System_Threading_Monitor_Monitor_pulse_all_raw,
// token 9888,
ves_icall_System_Threading_Monitor_Monitor_wait_raw,
// token 9890,
ves_icall_System_Threading_Monitor_Monitor_try_enter_with_atomic_var_raw,
// token 9943,
ves_icall_System_Threading_Thread_StartInternal_raw,
// token 9949,
ves_icall_System_Threading_Thread_InitInternal_raw,
// token 9950,
ves_icall_System_Threading_Thread_GetCurrentThread,
// token 9952,
ves_icall_System_Threading_InternalThread_Thread_free_internal_raw,
// token 9953,
ves_icall_System_Threading_Thread_GetState_raw,
// token 9954,
ves_icall_System_Threading_Thread_SetState_raw,
// token 9955,
ves_icall_System_Threading_Thread_ClrState_raw,
// token 9956,
ves_icall_System_Threading_Thread_SetName_icall_raw,
// token 9958,
ves_icall_System_Threading_Thread_YieldInternal,
// token 9960,
ves_icall_System_Threading_Thread_Join_internal_raw,
// token 9961,
ves_icall_System_Threading_Thread_SetPriority_raw,
// token 11260,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_PrepareForAssemblyLoadContextRelease_raw,
// token 11264,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_GetLoadContextForAssembly_raw,
// token 11266,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalLoadFile_raw,
// token 11267,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalInitializeNativeALC_raw,
// token 11268,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalLoadFromStream_raw,
// token 11269,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalGetLoadedAssemblies_raw,
// token 11534,
ves_icall_System_GCHandle_InternalAlloc_raw,
// token 11535,
ves_icall_System_GCHandle_InternalFree_raw,
// token 11536,
ves_icall_System_GCHandle_InternalGet_raw,
// token 11537,
ves_icall_System_GCHandle_InternalSet_raw,
// token 11557,
ves_icall_System_Runtime_InteropServices_Marshal_GetLastPInvokeError,
// token 11558,
ves_icall_System_Runtime_InteropServices_Marshal_SetLastPInvokeError,
// token 11559,
ves_icall_System_Runtime_InteropServices_Marshal_StructureToPtr_raw,
// token 11561,
ves_icall_System_Runtime_InteropServices_Marshal_SizeOfHelper_raw,
// token 11637,
ves_icall_System_Runtime_InteropServices_NativeLibrary_LoadByName_raw,
// token 11729,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InternalGetHashCode_raw,
// token 11731,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InternalTryGetHashCode_raw,
// token 11733,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetObjectValue_raw,
// token 11743,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetUninitializedObjectInternal_raw,
// token 11744,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray_raw,
// token 11745,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetSpanDataFrom_raw,
// token 11746,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_RunClassConstructor_raw,
// token 11747,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_SufficientExecutionStack,
// token 12227,
ves_icall_System_Reflection_Assembly_GetExecutingAssembly_raw,
// token 12228,
ves_icall_System_Reflection_Assembly_GetCallingAssembly_raw,
// token 12229,
ves_icall_System_Reflection_Assembly_GetEntryAssembly_raw,
// token 12234,
ves_icall_System_Reflection_Assembly_InternalLoad_raw,
// token 12235,
ves_icall_System_Reflection_Assembly_InternalGetType_raw,
// token 12314,
ves_icall_System_Reflection_AssemblyName_FreeAssemblyName,
// token 12315,
ves_icall_System_Reflection_AssemblyName_GetNativeName,
// token 12341,
ves_icall_MonoCustomAttrs_GetCustomAttributesInternal_raw,
// token 12348,
ves_icall_MonoCustomAttrs_GetCustomAttributesDataInternal_raw,
// token 12355,
ves_icall_MonoCustomAttrs_IsDefinedInternal_raw,
// token 12366,
ves_icall_System_Reflection_FieldInfo_internal_from_handle_type_raw,
// token 12369,
ves_icall_System_Reflection_FieldInfo_get_marshal_info_raw,
// token 12394,
ves_icall_System_Reflection_LoaderAllocatorScout_Destroy,
// token 12471,
ves_icall_System_Reflection_RuntimeAssembly_GetEntryPoint_raw,
// token 12484,
ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceNames_raw,
// token 12486,
ves_icall_System_Reflection_RuntimeAssembly_GetExportedTypes_raw,
// token 12488,
ves_icall_System_Reflection_RuntimeAssembly_GetTopLevelForwardedTypes_raw,
// token 12511,
ves_icall_System_Reflection_RuntimeAssembly_GetInfo_raw,
// token 12513,
ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceInfoInternal_raw,
// token 12514,
ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceInternal_raw,
// token 12515,
ves_icall_System_Reflection_Assembly_GetManifestModuleInternal_raw,
// token 12516,
ves_icall_System_Reflection_RuntimeAssembly_GetModulesInternal_raw,
// token 12517,
ves_icall_System_Reflection_Assembly_InternalGetReferencedAssemblies_raw,
// token 12526,
ves_icall_System_Reflection_RuntimeCustomAttributeData_ResolveArgumentsInternal_raw,
// token 12541,
ves_icall_RuntimeEventInfo_get_event_info_raw,
// token 12561,
ves_icall_reflection_get_token_raw,
// token 12562,
ves_icall_System_Reflection_EventInfo_internal_from_handle_type_raw,
// token 12570,
ves_icall_RuntimeFieldInfo_ResolveType_raw,
// token 12572,
ves_icall_RuntimeFieldInfo_GetParentType_raw,
// token 12579,
ves_icall_RuntimeFieldInfo_GetFieldOffset_raw,
// token 12580,
ves_icall_RuntimeFieldInfo_GetValueInternal_raw,
// token 12583,
ves_icall_RuntimeFieldInfo_SetValueInternal_raw,
// token 12585,
ves_icall_RuntimeFieldInfo_GetRawConstantValue_raw,
// token 12590,
ves_icall_reflection_get_token_raw,
// token 12596,
ves_icall_get_method_info_raw,
// token 12597,
ves_icall_get_method_attributes,
// token 12604,
ves_icall_System_Reflection_MonoMethodInfo_get_parameter_info_raw,
// token 12606,
ves_icall_System_MonoMethodInfo_get_retval_marshal_raw,
// token 12618,
ves_icall_System_Reflection_RuntimeMethodInfo_GetMethodFromHandleInternalType_native_raw,
// token 12621,
ves_icall_RuntimeMethodInfo_get_name_raw,
// token 12622,
ves_icall_RuntimeMethodInfo_get_base_method_raw,
// token 12623,
ves_icall_reflection_get_token_raw,
// token 12634,
ves_icall_InternalInvoke_raw,
// token 12643,
ves_icall_RuntimeMethodInfo_GetPInvoke_raw,
// token 12649,
ves_icall_RuntimeMethodInfo_MakeGenericMethod_impl_raw,
// token 12650,
ves_icall_RuntimeMethodInfo_GetGenericArguments_raw,
// token 12651,
ves_icall_RuntimeMethodInfo_GetGenericMethodDefinition_raw,
// token 12653,
ves_icall_RuntimeMethodInfo_get_IsGenericMethodDefinition_raw,
// token 12654,
ves_icall_RuntimeMethodInfo_get_IsGenericMethod_raw,
// token 12671,
ves_icall_InvokeClassConstructor_raw,
// token 12673,
ves_icall_InternalInvoke_raw,
// token 12687,
ves_icall_reflection_get_token_raw,
// token 12710,
ves_icall_System_Reflection_RuntimeModule_InternalGetTypes_raw,
// token 12711,
ves_icall_System_Reflection_RuntimeModule_GetGuidInternal_raw,
// token 12712,
ves_icall_System_Reflection_RuntimeModule_ResolveMethodToken_raw,
// token 12737,
ves_icall_RuntimeParameterInfo_GetTypeModifiers_raw,
// token 12742,
ves_icall_RuntimePropertyInfo_get_property_info_raw,
// token 12772,
ves_icall_reflection_get_token_raw,
// token 12773,
ves_icall_System_Reflection_RuntimePropertyInfo_internal_from_handle_type_raw,
// token 13343,
ves_icall_CustomAttributeBuilder_GetBlob_raw,
// token 13357,
ves_icall_DynamicMethod_create_dynamic_method_raw,
// token 13458,
ves_icall_AssemblyBuilder_basic_init_raw,
// token 13459,
ves_icall_AssemblyBuilder_UpdateNativeCustomAttributes_raw,
// token 13684,
ves_icall_ModuleBuilder_basic_init_raw,
// token 13685,
ves_icall_ModuleBuilder_set_wrappers_type_raw,
// token 13694,
ves_icall_ModuleBuilder_getUSIndex_raw,
// token 13695,
ves_icall_ModuleBuilder_getToken_raw,
// token 13696,
ves_icall_ModuleBuilder_getMethodToken_raw,
// token 13702,
ves_icall_ModuleBuilder_RegisterToken_raw,
// token 13799,
ves_icall_TypeBuilder_create_runtime_class_raw,
// token 14395,
ves_icall_System_IO_Stream_HasOverriddenBeginEndRead_raw,
// token 14396,
ves_icall_System_IO_Stream_HasOverriddenBeginEndWrite_raw,
// token 15011,
ves_icall_System_Diagnostics_Debugger_IsAttached_internal,
// token 15012,
ves_icall_System_Diagnostics_Debugger_IsLogging,
// token 15013,
ves_icall_System_Diagnostics_Debugger_Log,
// token 15018,
ves_icall_System_Diagnostics_StackFrame_GetFrameInfo,
// token 15028,
ves_icall_System_Diagnostics_StackTrace_GetTrace,
// token 16075,
ves_icall_Mono_RuntimeClassHandle_GetTypeFromClass,
// token 16096,
ves_icall_Mono_RuntimeGPtrArrayHandle_GPtrArrayFree,
// token 16098,
ves_icall_Mono_SafeStringMarshal_StringToUtf8,
// token 16100,
ves_icall_Mono_SafeStringMarshal_GFree,
};
static uint8_t corlib_icall_flags [] = {
0,
0,
0,
0,
0,
4,
4,
0,
4,
0,
4,
4,
4,
0,
0,
0,
4,
4,
4,
4,
4,
0,
4,
0,
0,
0,
4,
0,
4,
4,
4,
4,
0,
4,
4,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
0,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
0,
0,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
0,
0,
0,
0,
0,
0,
0,
0,
};
