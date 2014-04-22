﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Ajax.Utilities;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Parsing {
    public class ErrorSink {
        private Dictionary<string, string> m_reportedVariables;

        public virtual void ReportUndefined(Lookup lookup) {
        }

        public virtual void OnError(JScriptExceptionEventArgs error) {

        }

        internal void HandleUndeclaredVariable(string name, TokenWithSpan context) {
            if (!HasAlreadySeenErrorFor(name)) {
                HandleError(JSError.UndeclaredVariable, context);
            }
        }

        internal void HandleUndeclaredFunction(string name, TokenWithSpan context) {
            if (!HasAlreadySeenErrorFor(name)) {
                HandleError(JSError.UndeclaredFunction, context);
            }
        }

        internal void HandleError(JSError errorId, TokenWithSpan context, bool forceToError = false) {
            var error = new JScriptException(errorId, context);

            if (forceToError) {
                error.IsError = true;
            } else {
                error.IsError = error.Severity < 2;
            }

            OnCompilerError(error);
        }

        internal bool OnCompilerError(JScriptException se) {
            // format the error code
            string errorCode = "JS{0}".FormatInvariant((int)se.ErrorCode);

            OnError(
                new JScriptExceptionEventArgs(
                    se,
                    new ContextError(
                        se.IsError,
                        se.Severity,
                        GetSeverityString(se.Severity),
                        errorCode,
                        se.HelpLink,
                        se.Line,
                        se.Column,
                        se.EndLine,
                        se.EndColumn,
                        se.Message
                    )
                )
            );

            //true means carry on with compilation.
            return se.CanRecover;
        }

        private static string GetSeverityString(int severity) {
            // From jscriptexception.js:
            //
            //guide: 0 == there will be a run-time error if this code executes
            //       1 == the programmer probably did not intend to do this
            //       2 == this can lead to problems in the future.
            //       3 == this can lead to performance problems
            //       4 == this is just not right
            switch (severity) {
                case 0:
                    return JScript.Severity0;

                case 1:
                    return JScript.Severity1;

                case 2:
                    return JScript.Severity2;

                case 3:
                    return JScript.Severity3;

                case 4:
                    return JScript.Severity4;

                default:
                    return JScript.SeverityUnknown.FormatInvariant(severity);
            }
        }
        internal bool HasAlreadySeenErrorFor(String varName) {
            if (m_reportedVariables == null) {
                m_reportedVariables = new Dictionary<string, string>();
            } else if (m_reportedVariables.ContainsKey(varName)) {
                return true;
            }
            m_reportedVariables.Add(varName, varName);
            return false;
        }
    }
}