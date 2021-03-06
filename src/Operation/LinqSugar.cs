﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System
{
    public static class LinqSugar
    {
        [DebuggerHidden]
        public static Operation<V> SelectMany<T, U, V>(this Operation<T> operation, Func<T, Operation<U>> process, Func<T, U, V> projection)
        {
            if (operation.Succeeded)
            {
                var op2 = process(operation.Result);
                if (op2.Succeeded)
                {
                    return Operation.Create(() => projection(operation.Result, op2.Result));
                }
                else
                {
                    return new Operation<V>(op2.GetException())
                    {
                        Succeeded = false,
                        Message = op2.Message,
                        Result = default(V)
                    };
                }
            }
            return new Operation<V>(operation.GetException())
            {
                Succeeded = false,
                Result = default(V),
                Message = operation.Message,
            };
        }

        [DebuggerHidden]
        public static Operation<U> Select<T, U>(this Operation<T> operation, Func<T, U> process)
        {
            return OperationExtensions.Next(operation, process);
        }
        
        //Linq with Operation
        [DebuggerHidden]
        public static Operation<U> SelectMany<T, U>(this Operation operation, Func<object, Operation<T>> process, Func<object, T, U> projection)
        {
            var op1 = new Operation<object>(operation.GetException())
            {
                Message = operation.Message,
                Result = null,
                Succeeded = operation.Succeeded
            };
            return SelectMany(op1, process, projection);
        }

        [DebuggerHidden]
        public static Operation<U> SelectMany<T, U>(this Operation<T> operation, Func<T, Operation> process, Func<T, object, U> projection)
        {            
            if (operation.Succeeded)
            {
                var op2 = process(operation.Result);
                return op2.Next(() => projection(operation.Result, null));
            }

            return new Operation<U>(operation.GetException())
            {
                Succeeded = false,
                Result = default(U),
                Message = operation.Message,
            };
        }

        [DebuggerHidden]
        public static Operation<T> Select<T>(this Operation operation, Func<object, T> process)
        {
            var op1 = new Operation<object>(operation.GetException())
            {
                Message = operation.Message,
                Result = null,
                Succeeded = operation.Succeeded
            };
            return OperationExtensions.Next(op1, process);
        }

        // Linq with IEnumerable
        [DebuggerHidden]
        public static Operation<IEnumerable<V>> SelectMany<T, U, V>(this Operation<T> operation, Func<T, IEnumerable<U>> process, Func<T, U, V> projection)
        {
            if (operation.Succeeded)
            {
                var op2 = Operation.Create(() => process(operation.Result));
                 return op2.Next((enumerable) => enumerable.Select(x => projection(operation.Result, x)));
            }
            return new Operation<IEnumerable<V>>(operation.GetException())
            {
                Succeeded = false,
                Result = default(IEnumerable<V>),
                Message = operation.Message
            };
        }

        [DebuggerHidden]
        public static IEnumerable<Operation<V>> SelectMany<T, U, V>(this IEnumerable<T> operation, Func<T, Operation<U>> process, Func<T, U, V> projection)
        {
            return operation.Select(x => OperationExtensions.Next(process(x), u => projection(x, u)));
        }
    }
}
