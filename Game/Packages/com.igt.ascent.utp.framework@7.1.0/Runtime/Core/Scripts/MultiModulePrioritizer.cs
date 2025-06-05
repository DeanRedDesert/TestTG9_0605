// -----------------------------------------------------------------------
//  <copyright file = "MultiModulePrioritizer.cs" company = "IGT">
//      Copyright (c) 2020 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Utp.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Contains settings and logic for processing incoming UTP commands
    /// that target a module name that matches multiple enabled modules.
    /// </summary>
    public static class MultiModulePrioritizer
    {
        /// <summary>
        /// The full namespace (including class name) of the module currently being prioritized.
        /// </summary>
        public static string PrioritizedNamespace;

        /// <summary>
        /// The AutomationModule.Name property value of the module currently being prioritized.
        /// </summary>
        public static string PrioritizedModuleName;

        /// <summary>
        /// If false, an error will occur when the prioritized module is missing or disabled.  If true, the
        /// logic will attempt to look for another module of the same type (i.e. having the same base class).
        /// </summary>
        public static bool RevertToNextOnFailure;


        /// <summary>
        /// Handles any multi-module prioritization processing that may be needed for the command.
        /// If no special processing is needed, this method returns false to indiciate that nothing happened.
        /// If special processing does occur, this method returns true to indiciate that the caller needs to take no further action.
        /// </summary>
        /// <returns>
        /// True if processing is complete (either the command was executed or an error command was sent).
        /// False if processing should be continued by the caller.  The command wasn't executed for some reason,
        /// but either prioritization doesn't apply in this case or fallback to standard processing is acceptable.
        /// </returns>
        public static bool HandlePrioritizedExecution(List<AutomationModule> testModules, AutomationCommander commander)
        {
            //  If the settings haven't been configured, then this method doesn't apply
            if (string.IsNullOrEmpty(PrioritizedNamespace) || string.IsNullOrEmpty(PrioritizedModuleName))
                return false;

            //  If the currently configured PrioritizedModuleName doesn't match the command, then this method doesn't apply
            if (PrioritizedModuleName != commander.AutoCommand.Module)
                return false;

            //  Get all modules with a matching namespace & AutomationModule.Name
            var matchingModules = testModules.Where(m =>
            {
                var fullName = m.GetType().FullName;
                return fullName != null && fullName.Equals(PrioritizedNamespace, StringComparison.InvariantCultureIgnoreCase) && m.Name == PrioritizedModuleName;
            }).ToList();

            //  We can't execute here if there's not exactly 1 match
            if(matchingModules.Count != 1)
            {
                //  If exactly 1 match wasn't found, then returning false will indiciate to the caller to continue standard processing
                if(RevertToNextOnFailure)
                    return false;

                //  Since we can't RevertToNext, it means we need to error out... send an error command response and return true to let the caller know no further processing is needed
                var msg = "Multi-module prioritization failed. Expected exactly 1 module with full namespace '" + PrioritizedNamespace + "' with AutomationModule.Name '" +
                          PrioritizedModuleName + "' but found " + matchingModules.Count + ". Command execution was halted because RevertToNext is false.";
                UtpController.SendCannotProcess(commander.Communication, PrioritizedModuleName, msg);
                return true;
            }

            //  We can't execute here if the module is disabled
            if(matchingModules.First().ModuleStatus != ModuleStatuses.InitializedEnabled)
            {
                //  The prioritized module is disabled, so if RevertToNext is allowed then return false to indicate to the caller to continue standard processing
                if(RevertToNextOnFailure)
                    return false;

                //  Since we can't RevertToNext, it means we need to error out... send an error command response and return true to let the caller know no further processing is needed
                var msg = "Multi-module prioritization failed. Found a module with full namespace '" + PrioritizedNamespace + "' with AutomationModule.Name '"
                          + PrioritizedModuleName + "', but it was disabled. Command execution was halted because RevertToNext is false.";
                UtpController.SendCannotProcess(commander.Communication, PrioritizedModuleName, msg);
                return true;
            }
            
            //  We found the prioritized module and it's enabled, so execute the command & return true to indicate there's nothing left for the caller to do
            matchingModules.First().Execute(commander);
            return true;
        }
    }
}
