﻿using System;
using System.Collections.Generic;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Running;

namespace BenchmarkDotNet.Jobs
{
    public static class JobExtensions
    {
        public static Job With(this Job job, Platform platform) => job.WithCore(j => j.Env.Platform = platform);
        public static Job WithId(this Job job, string id) => new Job(id, job);

        // Env
        public static Job With(this Job job, Jit jit) => job.WithCore(j => j.Env.Jit = jit);
        public static Job With(this Job job, Runtime runtime) => job.WithCore(j => j.Env.Runtime = runtime);
        public static Job WithAffinity(this Job job, IntPtr affinity) => job.WithCore(j => j.Env.Affinity = affinity);
        public static Job WithGcServer(this Job job, bool value) => job.WithCore(j => j.Env.Gc.Server = value);
        public static Job WithGcConcurrent(this Job job, bool value) => job.WithCore(j => j.Env.Gc.Concurrent = value);
        public static Job WithGcCpuGroups(this Job job, bool value) => job.WithCore(j => j.Env.Gc.CpuGroups = value);
        public static Job WithGcForce(this Job job, bool value) => job.WithCore(j => j.Env.Gc.Force = value);
        public static Job WithGcAllowVeryLargeObjects(this Job job, bool value) => job.WithCore(j => j.Env.Gc.AllowVeryLargeObjects = value);
        public static Job WithGcRetainVm(this Job job, bool value) => job.WithCore(j => j.Env.Gc.RetainVm = value);
        public static Job With(this Job job, GcMode gc) => job.WithCore(j => EnvMode.GcCharacteristic[j] = gc);

        // Run
        public static Job With(this Job job, RunStrategy strategy) => job.WithCore(j => j.Run.RunStrategy = strategy);
        public static Job WithLaunchCount(this Job job, int count) => job.WithCore(j => j.Run.LaunchCount = count);
        public static Job WithWarmupCount(this Job job, int count) => job.WithCore(j => j.Run.WarmupCount = count);
        public static Job WithTargetCount(this Job job, int count) => job.WithCore(j => j.Run.TargetCount = count);
        public static Job WithIterationTime(this Job job, TimeInterval time) => job.WithCore(j => j.Run.IterationTime = time);
        public static Job WithInvocationCount(this Job job, int count) => job.WithCore(j => j.Run.InvocationCount = count);
        public static Job WithUnrollFactor(this Job job, int factor) => job.WithCore(j => j.Run.UnrollFactor = factor);
        public static Job RunOncePerIteration(this Job job) => job.WithInvocationCount(1).WithUnrollFactor(1);
        public static Job WithMinTargetIterationCount(this Job job, int count) => job.WithCore(j => j.Run.MinTargetIterationCount = count);
        public static Job WithMaxTargetIterationCount(this Job job, int count) => job.WithCore(j => j.Run.MaxTargetIterationCount = count);

        // Infrastructure
        public static Job With(this Job job, IToolchain toolchain) => job.WithCore(j => j.Infrastructure.Toolchain = toolchain);
        public static Job With(this Job job, IClock clock) => job.WithCore(j => j.Infrastructure.Clock = clock);
        public static Job With(this Job job, IEngineFactory engineFactory) => job.WithCore(j => j.Infrastructure.EngineFactory = engineFactory);
        public static Job WithCustomBuildConfiguration(this Job job, string buildConfiguration) => job.WithCore(j => j.Infrastructure.BuildConfiguration = buildConfiguration);
        public static Job With(this Job job, IReadOnlyList<EnvironmentVariable> environmentVariables) => job.WithCore(j => j.Infrastructure.EnvironmentVariables = environmentVariables);
        public static Job With(this Job job, IReadOnlyList<Argument> arguments) => job.WithCore(j => j.Infrastructure.Arguments = arguments);

        // Accuracy
        public static Job WithMaxRelativeError(this Job job, double value) => job.WithCore(j => j.Accuracy.MaxRelativeError= value);
        public static Job WithMaxAbsoluteError(this Job job, TimeInterval value) => job.WithCore(j => j.Accuracy.MaxAbsoluteError = value);
        public static Job WithMinIterationTime(this Job job, TimeInterval value) => job.WithCore(j => j.Accuracy.MinIterationTime = value);
        public static Job WithMinInvokeCount(this Job job, int value) => job.WithCore(j => j.Accuracy.MinInvokeCount = value);
        public static Job WithEvaluateOverhead(this Job job, bool value) => job.WithCore(j => j.Accuracy.EvaluateOverhead = value);
        public static Job WithOutlierMode(this Job job, OutlierMode value) => job.WithCore(j => j.Accuracy.OutlierMode = value);
        [Obsolete("Please use the new WithOutlierMode instead")]
        public static Job WithRemoveOutliers(this Job job, bool value) => job.WithCore(j => j.Accuracy.OutlierMode = value ? OutlierMode.OnlyUpper : OutlierMode.None);
        public static Job WithAnalyzeLaunchVariance(this Job job, bool value) => job.WithCore(j => j.Accuracy.AnalyzeLaunchVariance = value);
        
        // Meta
        public static Job AsBaseline(this Job job) => job.WithCore(j => j.Meta.IsBaseline = true);
        public static Job WithIsBaseline(this Job job, bool value) => value ? job.AsBaseline() : job;

        internal static Job MakeSettingsUserFriendly(this Job job, Target target)
        {
            // users expect that if IterationSetup is configured, it should be run before every benchmark invocation https://github.com/dotnet/BenchmarkDotNet/issues/730
            if (target.IterationSetupMethod != null
                && !job.HasValue(RunMode.InvocationCountCharacteristic)
                && !job.HasValue(RunMode.UnrollFactorCharacteristic))
            {
                return job.RunOncePerIteration();
            }

            return job;
        }

        private static Job WithCore(this Job job, Action<Job> updateCallback)
        {
            var hasId = job.HasValue(Job.IdCharacteristic);

            var newJob = hasId ? new Job(job.Id, job) : new Job(job);
            updateCallback(newJob);
            return newJob;
        }
    }
}
