using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading;

namespace HQ.Flow
{
	public class StateProvider
    {
	    private const string StateDisambiguatorPrefix = "State_";

	    public interface IHaveSymbol
        {
            string Symbol { get; }
        }

        public class MethodTable { }

        public class State
        {
            public MethodTable methodTable;
        }

        #region Lookup Helpers

        private static class StateInstanceLookup<TState> where TState : State, new()
        {
            internal static readonly Dictionary<Type, TState> ForType = new Dictionary<Type, TState>();

	        internal static void Add(Type stateMachineType, TState state)
            {
                ForType.Add(stateMachineType, state);
            }

			internal static TState Get(Type stateMachineType)
			{
				ForType.TryGetValue(stateMachineType, out var result);
				return result;
			}

			internal static void Clear()
	        {
		        ForType.Clear();
	        }
		}

        public TState GetState<TState>() where TState : State, new()
        {
            var type = GetType();

            return StateInstanceLookup<TState>.ForType[type];
        }


        public static TState GetState<TType, TState>()
            where TType : StateProvider
            where TState : State, new()
        {
            return StateInstanceLookup<TState>.ForType[typeof (TType)];
        }

        public static ReadOnlyCollection<State> GetAllStatesFor(Type type)
        {
            return new List<State>(_allStatesByType[type]).AsReadOnly();
        }
		
        public State GetStateBySymbol(string symbol)
        {
            var type = GetType();

            foreach (var state in _allStatesByType[type])
            {
	            if(!(state is IHaveSymbol queryable))
                    continue;

                if(queryable.Symbol == symbol)
                    return state;
            }
            return null;
        }

        public static List<State> DeveloperGetAllStatesByType(Type type)
        {
            return _allStatesByType[type];
        }

        #endregion

	    public static List<State> AllStateInstances
	    {
		    get
		    {
			    Debug.Assert(_allStateInstances != null);
				return _allStateInstances;
			}
	    }

	    private static List<State> _allStateInstances;
	    private static Dictionary<Type, List<State>> _allStatesByType;

		#region Setup

	    /// <summary>Initialize all state machines. </summary>
	    public static void Setup()
	    {
		    Setup((IEnumerable<Assembly>)AppDomain.CurrentDomain.GetAssemblies());
		}

	    /// <summary>Initialize all state machines. </summary>
		public static void Setup(params Assembly[] assemblies)
	    {
			Setup((IEnumerable<Assembly>)assemblies);
		}

		/// <summary>Initialize all state machines. </summary>
		public static void Setup(IEnumerable<Assembly> assemblies)
        {
            var types = assemblies.SelectMany(a => a.GetTypes());

			Setup(types);
        }
		
	    /// <summary>Initialize all state machines. </summary>
	    public static void Setup(params Type[] types)
	    {
			Setup((IEnumerable<Type>)types);
		}

	    /// <summary>Initialize all state machines. </summary>
	    public static void Setup<T>()
	    {
		    Setup(typeof(T));
	    }

	    /// <summary>Initialize all state machines. </summary>
	    public static void Setup(IEnumerable<Type> types)
	    {
		    Initialize();

		    var stateMachinesToStates = new Dictionary<Type, Dictionary<Type, State>>();
		    var stateMachinesToAbstractStates = new Dictionary<Type, Dictionary<Type, MethodTable>>();

		    foreach (var type in types.Where(t => typeof(StateProvider).IsAssignableFrom(t)))
		    {
			    SetupStateMachineTypeRecursive(stateMachinesToStates, stateMachinesToAbstractStates, type);
		    }

		    _allStatesByType = new Dictionary<Type, List<State>>();
		    foreach (var stateMachineAndStates in stateMachinesToStates.NetworkOrder(kvp => kvp.Key.ToString()))
		    {
			    foreach (var state in stateMachineAndStates.Value.NetworkOrder(kvp => kvp.Key.ToString()))
			    {
				    AllStateInstances.Add(state.Value);

				    if (!_allStatesByType.TryGetValue(stateMachineAndStates.Key, out var states))
				    {
					    states = new List<State>();
					    _allStatesByType.Add(stateMachineAndStates.Key, states);
				    }

				    _allStatesByType[stateMachineAndStates.Key].Add(state.Value);

				    var stateInstanceLookupType = typeof(StateInstanceLookup<>).MakeGenericType(state.Key);

				    const BindingFlags bindingFlags =
					    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
				    stateInstanceLookupType.GetMethod("Add", bindingFlags)?
					    .Invoke(null, new object[] {stateMachineAndStates.Key, state.Value});
			    }
		    }
	    }

	    private static void Initialize()
	    {
		    if (Interlocked.CompareExchange(ref _allStateInstances, new List<State>(), null) != null)
		    {
			    throw new AlreadyInitializedException();
		    }
	    }

		private static string GetStateName(MemberInfo stateType)
        {
            var stateName = stateType.Name;
            if(stateName != "State" && stateName.EndsWith("State"))
                stateName = stateName.Substring(0, stateName.Length - "State".Length);
            return stateName;
        }
		
        private static void SetupStateMachineTypeRecursive(IDictionary<Type, Dictionary<Type, State>> stateMachinesToStates,
                IDictionary<Type, Dictionary<Type, MethodTable>> stateMachinesToAbstractStates,
                Type stateMachineType)
        {
            Debug.Assert(typeof(StateProvider).IsAssignableFrom(stateMachineType));

            if(stateMachinesToStates.ContainsKey(stateMachineType))
                return;

            Dictionary<Type, State> baseStates = null;
            Dictionary<Type, MethodTable> baseAbstractStates = null;
            if(stateMachineType != typeof(StateProvider))
            {
                SetupStateMachineTypeRecursive(stateMachinesToStates, stateMachinesToAbstractStates, stateMachineType.BaseType);
                baseStates = stateMachinesToStates[stateMachineType.BaseType ?? throw new InvalidOperationException("MethodTable base type not found")];
                baseAbstractStates = stateMachinesToAbstractStates[stateMachineType.BaseType];
            }

	        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

	        var typeMethods = stateMachineType.GetMethods(bindingFlags);
	        var stateMethods = typeMethods.ToDictionary(mi => mi.Name);

			Type methodTableType;
            var methodTableSearchType = stateMachineType;
            while((methodTableType = methodTableSearchType?.GetNestedType("MethodTable", BindingFlags.Public | BindingFlags.NonPublic)) == null)
            {
                if(!typeof(StateProvider).IsAssignableFrom(methodTableSearchType?.BaseType))
                    break;
                methodTableSearchType = methodTableSearchType?.BaseType;
            }

            if(methodTableType == null)
                throw new InvalidOperationException($"MethodTable not found for {stateMachineType}");

            if(!typeof(MethodTable).IsAssignableFrom(methodTableType))
                throw new InvalidOperationException("MethodTable must be derived from StateMachine.MethodTable");

            var states = new Dictionary<Type, State>();
            var abstractStates = new Dictionary<Type, MethodTable>();

            Debug.Assert(baseStates != null == (baseAbstractStates != null));
            if(baseStates != null)
            {
	            foreach (var baseState in baseStates)
                {
                    var state = (State)Activator.CreateInstance(baseState.Key);
                    state.methodTable = ShallowCloneToDerived(baseState.Value.methodTable, methodTableType, stateMachineType);
                    FillMethodTableWithOverrides(baseState.Key, state.methodTable, stateMachineType, stateMethods);
                    states.Add(baseState.Key, state);
                }

                foreach(var baseAbstractState in baseAbstractStates)
                {
                    var methodTable = ShallowCloneToDerived(baseAbstractState.Value, methodTableType, stateMachineType);
                    FillMethodTableWithOverrides(baseAbstractState.Key, methodTable, stateMachineType, stateMethods);
                    abstractStates.Add(baseAbstractState.Key, methodTable);
                }
            }

            var newStateTypes = stateMachineType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Where(nt => typeof(State).IsAssignableFrom(nt)).ToArray();
            foreach(var stateType in newStateTypes)
            {
                SetupStateTypeRecursive(states, abstractStates, stateType, stateMachineType, methodTableType, stateMethods);
            }

	        var stateTypesToMethodTables = states
		        .Select(kvp => new KeyValuePair<Type, MethodTable>(kvp.Key, kvp.Value.methodTable))
		        .Concat(abstractStates).ToList();
			
			foreach (var typeToMethodTable in stateTypesToMethodTables)
	        {
		        var methodTable = typeToMethodTable.Value;
				var allMethodTableEntries = methodTable.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
			        .Where(fi => fi.FieldType.BaseType == typeof(MulticastDelegate)).ToList();

		        if (!allMethodTableEntries.Any())
			        stateMethods.Clear();

		        var toRemove = new List<string>(stateMethods.Keys);
		        foreach (var fieldInfo in allMethodTableEntries)
		        {
			        foreach (var stateMethod in stateMethods)
			        {
				        if (Regex.IsMatch(stateMethod.Key, $@"{StateDisambiguatorPrefix}_\w*_{fieldInfo.Name}") || Regex.IsMatch(stateMethod.Key, $@"\w*_{fieldInfo.Name}"))
					        toRemove.Remove(stateMethod.Key);
					}
		        }

		        foreach (var stateMethod in toRemove)
			        stateMethods.Remove(stateMethod);
	        }
			
	        if (stateMethods.Count > 0)
            {
                throw new UnusedStateMethodsException(stateMethods.Values);
            }
			
            foreach(var typeToMethodTable in stateTypesToMethodTables)
            {
                MethodTable methodTable = typeToMethodTable.Value;
                var allMethodTableEntries = methodTable.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                        .Where(fi => fi.FieldType.BaseType == typeof(MulticastDelegate));

                foreach(var fieldInfo in allMethodTableEntries)
                {
                    if(fieldInfo.GetCustomAttributes(typeof(AlwaysNullCheckedAttribute), true).Length != 0)
                        continue;

                    if(fieldInfo.GetValue(methodTable) == null)
                    {
                        var methodInMethodTable = fieldInfo.FieldType.GetMethod("Invoke");
	                    Debug.Assert(methodInMethodTable != null, nameof(methodInMethodTable) + " != null");

						DynamicMethod dynamicMethod = new DynamicMethod($"DoNothing_{GetStateName(typeToMethodTable.Key)}_{fieldInfo.Name}", methodInMethodTable.ReturnType,
                                methodInMethodTable.GetParameters().Select(pi => pi.ParameterType).ToArray(), stateMachineType);

                        var il = dynamicMethod.GetILGenerator();
	                    EmitDefault(il, methodInMethodTable.ReturnType);
                        il.Emit(OpCodes.Ret);

                        fieldInfo.SetValue(methodTable, dynamicMethod.CreateDelegate(fieldInfo.FieldType));
                    }
                }
            }
			
            stateMachinesToStates.Add(stateMachineType, states);
            stateMachinesToAbstractStates.Add(stateMachineType, abstractStates);
        }

        private static void EmitDefault(ILGenerator il, Type type)
        {
            if(type == typeof(void))
                return;

            switch(Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Conv_I8);
                    break;

                case TypeCode.Single:
                    il.Emit(OpCodes.Ldc_R4, default(float));
                    break;

                case TypeCode.Double:
                    il.Emit(OpCodes.Ldc_R8, default(double));
                    break;

                default:
                    if(type.IsValueType)
                    {
                        var lb = il.DeclareLocal(type);
                        il.Emit(OpCodes.Ldloca, lb);
                        il.Emit(OpCodes.Initobj, type);
                        il.Emit(OpCodes.Ldloc, lb);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldnull);
                    }
                    break;
            }
        }

		private static void SetupStateTypeRecursive(IDictionary<Type, State> states, IDictionary<Type, MethodTable> abstractStates,
                Type stateType, Type stateMachineType, Type methodTableType, Dictionary<string, MethodInfo> stateMethods)
        {
            if(states.ContainsKey(stateType) || abstractStates.ContainsKey(stateType))
                return;

            if(stateType == typeof(State) && stateMachineType == typeof(StateProvider))
            {
                Debug.Assert(!stateType.IsAbstract);
                states.Add(stateType, new State { methodTable = new MethodTable() });
                return;
            }

            Debug.Assert(stateType != typeof(State));

            SetupStateTypeRecursive(states, abstractStates, stateType.BaseType, stateMachineType, methodTableType, stateMethods);

	        Debug.Assert(stateType.BaseType != null, "stateType.BaseType != null");

	        var parentMethodTable = stateType.BaseType.IsAbstract
                    ? abstractStates[stateType.BaseType]
                    : states[stateType.BaseType].methodTable;
            var methodTable = ShallowCloneToDerived(parentMethodTable, methodTableType, stateMachineType);
            FillMethodTableWithOverrides(stateType, methodTable, stateMachineType, stateMethods);

			if(stateType.IsAbstract)
            {
                abstractStates.Add(stateType, methodTable);
            }
            else
            {
                var state = (State)Activator.CreateInstance(stateType);
                state.methodTable = methodTable;
                states.Add(stateType, state);
            }
        }

		private static MethodTable ShallowCloneToDerived(MethodTable state, Type derivedType, Type stateMachineType)
        {
	        if (derivedType.IsGenericType)
	        {
		        var typeSource = stateMachineType.IsGenericType ? stateMachineType : state.GetType();
		        derivedType = derivedType.MakeGenericType(typeSource.GetGenericArguments()[0]);
	        }

			var baseType = state.GetType();
            if(!baseType.IsAssignableFrom(derivedType))
                throw new Exception("Method table inheritance hierarchy error.");

	        if (derivedType.ContainsGenericParameters)
		        return state;
			
	        var derivedMethodTable = (MethodTable)Activator.CreateInstance(derivedType);
	        foreach (var field in baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                field.SetValue(derivedMethodTable, field.GetValue(state));
            }

            return derivedMethodTable;
        }
		
        private static void FillMethodTableWithOverrides(Type stateType, MethodTable methodTable, Type stateMachineType, Dictionary<string, MethodInfo> stateMethods)
        {
            var allMethodTableEntries = methodTable.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(fi => fi.FieldType.BaseType == typeof(MulticastDelegate));

            foreach(var fieldInfo in allMethodTableEntries)
            {
	            var naturalName = $"{GetStateName(stateType)}_{fieldInfo.Name}";
	            var disambiguatedName = $"{StateDisambiguatorPrefix}{naturalName}";

	            if (stateMethods.TryGetValue(naturalName, out var methodInStateMachine))
	            {
		            PotentialMethodNameMatch(methodTable, stateMachineType, stateMethods, fieldInfo, methodInStateMachine, naturalName);
	            }

	            if (stateMethods.TryGetValue(disambiguatedName, out methodInStateMachine))
	            {
		            PotentialMethodNameMatch(methodTable, stateMachineType, stateMethods, fieldInfo, methodInStateMachine, disambiguatedName);
	            }
			}
        }

	    private static void PotentialMethodNameMatch(MethodTable methodTable, Type stateMachineType, Dictionary<string, MethodInfo> stateMethods,
		    FieldInfo fieldInfo, MethodInfo methodInStateMachine, string potentialMethodName)
	    {
		    var methodInMethodTable = fieldInfo.FieldType.GetMethod("Invoke");
		    Debug.Assert(methodInMethodTable != null, nameof(methodInMethodTable) + " != null");

		    if (methodInStateMachine.ReturnType != methodInMethodTable.ReturnType)
			    ThrowMethodMismatch(methodInStateMachine, methodInMethodTable);

		    var methodInMethodTableParameters = methodInMethodTable.GetParameters();
		    var methodInStateMachineParameters = methodInStateMachine.GetParameters();

		    if (methodInStateMachineParameters.Length != methodInMethodTableParameters.Length - 1
		    ) // -1 to account for 'this' parameter to open delegate
			    ThrowMethodMismatch(methodInStateMachine, methodInMethodTable);

		    for (var i = 0; i < methodInStateMachineParameters.Length; i++)
			    if (methodInStateMachineParameters[i].ParameterType !=
			        methodInMethodTableParameters[i + 1]
				        .ParameterType && // +1 to account for 'this' parameter to open delegate     
			        !methodInMethodTableParameters[i + 1].ParameterType
				        .IsAssignableFrom(methodInStateMachineParameters[i].ParameterType)
			    ) // i.e. supports custom implementations of IUpdateContext
				    ThrowMethodMismatch(methodInStateMachine, methodInMethodTable);

		    if (!stateMachineType.IsAssignableFrom(methodInMethodTableParameters[0].ParameterType))
		    {
			    Debug.Assert(methodInMethodTableParameters[0].ParameterType.IsAssignableFrom(stateMachineType));
			    DynamicMethod dynamicMethod = new DynamicMethod($"CastingShim_{potentialMethodName}",
				    methodInMethodTable.ReturnType, methodInMethodTableParameters.Select(pi => pi.ParameterType).ToArray(),
				    stateMachineType);
			    var il = dynamicMethod.GetILGenerator();
			    il.Emit(OpCodes.Ldarg_0);
			    il.Emit(OpCodes.Castclass, stateMachineType); // <- the casting bit of the shim
			    if (methodInMethodTableParameters.Length > 1) il.Emit(OpCodes.Ldarg_1);
			    if (methodInMethodTableParameters.Length > 2) il.Emit(OpCodes.Ldarg_2);
			    if (methodInMethodTableParameters.Length > 3) il.Emit(OpCodes.Ldarg_3);
			    for (var i = 4; i < methodInMethodTableParameters.Length; i++)
			    {
				    if (i <= byte.MaxValue)
					    il.Emit(OpCodes.Ldarg_S, (byte) i);
				    else
					    il.Emit(OpCodes.Ldarg, (ushort) i);
			    }

			    il.Emit(OpCodes.Callvirt, methodInStateMachine);
			    il.Emit(OpCodes.Ret);

			    fieldInfo.SetValue(methodTable, dynamicMethod.CreateDelegate(fieldInfo.FieldType));
		    }
		    else
		    {
			    fieldInfo.SetValue(methodTable, Delegate.CreateDelegate(fieldInfo.FieldType, methodInStateMachine));
		    }

		    stateMethods.Remove(potentialMethodName);
	    }

	    private static void ThrowMethodMismatch(MethodInfo methodInStateMachine, MethodInfo methodInMethodTable)
        {
            throw new Exception($"Method signature does not match: \"{methodInStateMachine}\" cannot be used for \"{methodInMethodTable}\"");
        }

        #endregion

	    public static void Clear()
	    {
			if (_allStateInstances != null)
		    {
			    foreach (var state in _allStateInstances)
			    {
				    var stateInstanceLookupType = typeof(StateInstanceLookup<>).MakeGenericType(state.GetType());
				    const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
				    var clearMethod = stateInstanceLookupType.GetMethod("Clear", bindingFlags);
				    clearMethod?.Invoke(null, null);
			    }
		    }

		    _allStatesByType?.Clear();
		    _allStatesByType = null;

		    _allStateInstances?.Clear();
		    _allStateInstances = null;
		}
    }
}
