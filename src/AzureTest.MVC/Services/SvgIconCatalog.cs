using System;
using System.Collections.Generic;
using System.Linq;
using AzureTest.MVC.Models;

namespace AzureTest.MVC.Services;

public static class SvgIconCatalog
{
    private static readonly List<SvgIconItem> _icons = new()
    {
        // ─── 1. POMODORO & TIMERS ───
        new SvgIconItem
        {
            Key = "pomodoro-focus",
            Name = "Pomodoro Focus",
            Category = "Pomodoro & Timers",
            Description = "Tomato focus timer emblem with leaf stem and subtle tick indicators.",
            Tags = new[] { "pomodoro", "tomato", "focus", "work", "timer", "deep work" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M12 2v4""/><path d=""M10 3c-1.5 0-3 .8-3 2 0 1.5 2 2 5 2s5-.5 5-2c0-1.2-1.5-2-3-2""/><circle cx=""12"" cy=""14"" r=""8""/><path d=""M12 10v4l2.5 2.5""/></svg>"
        },
        new SvgIconItem
        {
            Key = "coffee-break",
            Name = "Coffee Short Break",
            Category = "Pomodoro & Timers",
            Description = "Steaming hot coffee cup for short intervals and rests.",
            Tags = new[] { "coffee", "tea", "break", "rest", "cup", "short break" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M17 8h1a4 4 0 0 1 0 8h-1""/><path d=""M3 8h14v9a4 4 0 0 1-4 4H7a4 4 0 0 1-4-4Z""/><line x1=""6"" x2=""6"" y1=""2"" y2=""4""/><line x1=""10"" x2=""10"" y1=""2"" y2=""4""/><line x1=""14"" x2=""14"" y1=""2"" y2=""4""/></svg>"
        },
        new SvgIconItem
        {
            Key = "palm-long-break",
            Name = "Palm Long Break",
            Category = "Pomodoro & Timers",
            Description = "Tropical palm tree representing restorative long breaks and rejuvenation.",
            Tags = new[] { "palm", "tree", "vacation", "rest", "long break", "relax" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M13 8c0-2.8-2.2-5-5-5 0 2.8 2.2 5 5 5Z""/><path d=""M13 8c0-2.8 2.2-5 5-5 0 2.8-2.2 5-5 5Z""/><path d=""M13 8c-2.8 0-5 2.2-5 5 2.8 0 5-2.2 5-5Z""/><path d=""M13 8c2.8 0 5 2.2 5 5-2.8 0-5-2.2-5-5Z""/><path d=""M12 8v14""/><path d=""M8 22h8""/></svg>"
        },
        new SvgIconItem
        {
            Key = "digital-clock",
            Name = "Digital Display",
            Category = "Pomodoro & Timers",
            Description = "Digital clock face with segmented digits and time colons.",
            Tags = new[] { "digital", "clock", "display", "led", "electronic" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><rect width=""20"" height=""14"" x=""2"" y=""5"" rx=""3""/><line x1=""6"" x2=""6"" y1=""9"" y2=""15""/><line x1=""10"" x2=""10"" y1=""9"" y2=""15""/><circle cx=""12"" cy=""11"" r=""0.5"" fill=""currentColor""/><circle cx=""12"" cy=""13"" r=""0.5"" fill=""currentColor""/><line x1=""14"" x2=""14"" y1=""9"" y2=""15""/><line x1=""18"" x2=""18"" y1=""9"" y2=""15""/></svg>"
        },
        new SvgIconItem
        {
            Key = "analog-clock",
            Name = "Analog Clock Face",
            Category = "Pomodoro & Timers",
            Description = "Classic analog watch dial with hour, minute indicators.",
            Tags = new[] { "analog", "clock", "dial", "time", "watch" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><circle cx=""12"" cy=""12"" r=""10""/><polyline points=""12 6 12 12 16 14""/></svg>"
        },
        new SvgIconItem
        {
            Key = "hourglass-timer",
            Name = "Sand Glass Hourglass",
            Category = "Pomodoro & Timers",
            Description = "Hourglass with flowing sand granules and wooden frame.",
            Tags = new[] { "sand", "hourglass", "sandglass", "elapsed", "countdown" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M5 22h14""/><path d=""M5 2h14""/><path d=""M17 22v-4.172a2 2 0 0 0-.586-1.414L12 12l-4.414 4.414A2 2 0 0 0 7 17.828V22""/><path d=""M7 2v4.172a2 2 0 0 0 .586 1.414L12 12l4.414-4.414A2 2 0 0 0 17 6.172V2""/></svg>"
        },
        new SvgIconItem
        {
            Key = "kitchen-timer",
            Name = "Kitchen Rotary Timer",
            Category = "Pomodoro & Timers",
            Description = "Mechanical rotary kitchen timer dial with top marker notch.",
            Tags = new[] { "kitchen", "rotary", "mechanical", "dial", "cook" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><circle cx=""12"" cy=""13"" r=""9""/><path d=""M12 4v2""/><path d=""M12 10v3l2 2""/><path d=""M10 2h4""/></svg>"
        },
        new SvgIconItem
        {
            Key = "candle-flame",
            Name = "Candle Flame",
            Category = "Pomodoro & Timers",
            Description = "Melting candle with illuminated glowing wick and flame.",
            Tags = new[] { "candle", "flame", "fire", "light", "wax", "focus" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M12 2s-3 2.5-3 4.5a3 3 0 0 0 6 0c0-2-3-4.5-3-4.5Z""/><line x1=""12"" x2=""12"" y1=""9"" y2=""11""/><rect width=""8"" height=""11"" x=""8"" y=""11"" rx=""2""/><line x1=""6"" x2=""18"" y1=""22"" y2=""22""/></svg>"
        },
        new SvgIconItem
        {
            Key = "fullscreen-expand",
            Name = "Fullscreen Expand",
            Category = "Pomodoro & Timers",
            Description = "Distraction-free full-screen maximize arrows.",
            Tags = new[] { "fullscreen", "expand", "maximize", "view", "arrows" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M8 3H5a2 2 0 0 0-2 2v3""/><path d=""M21 8V5a2 2 0 0 0-2-2h-3""/><path d=""M3 16v3a2 2 0 0 0 2 2h3""/><path d=""M16 21h3a2 2 0 0 0 2-2v-3""/></svg>"
        },
        new SvgIconItem
        {
            Key = "fullscreen-compress",
            Name = "Fullscreen Exit",
            Category = "Pomodoro & Timers",
            Description = "Exit full-screen mode inward compression arrows.",
            Tags = new[] { "fullscreen", "exit", "compress", "minimize", "screen" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M4 14h6v6""/><path d=""M20 10h-6V4""/><path d=""M14 10l7-7""/><path d=""M10 14l-7 7""/></svg>"
        },
        new SvgIconItem
        {
            Key = "timer-play",
            Name = "Play / Start Focus",
            Category = "Pomodoro & Timers",
            Description = "Start or resume countdown button icon.",
            Tags = new[] { "play", "start", "run", "begin" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><polygon points=""6 3 20 12 6 21 6 3""/></svg>"
        },
        new SvgIconItem
        {
            Key = "timer-pause",
            Name = "Pause Focus",
            Category = "Pomodoro & Timers",
            Description = "Pause active timer session.",
            Tags = new[] { "pause", "hold", "stop" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><rect x=""6"" y=""4"" width=""4"" height=""16"" rx=""1""/><rect x=""14"" y=""4"" width=""4"" height=""16"" rx=""1""/></svg>"
        },
        new SvgIconItem
        {
            Key = "timer-reset",
            Name = "Reset Timer",
            Category = "Pomodoro & Timers",
            Description = "Counter-clockwise circular reset arrow.",
            Tags = new[] { "reset", "replay", "restart", "counter" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8""/><path d=""M3 3v5h5""/></svg>"
        },

        // ─── 2. HABITS & PRODUCTIVITY ───
        new SvgIconItem
        {
            Key = "streak-flame",
            Name = "Streak Flame",
            Category = "Habits & Productivity",
            Description = "Consecutive day streak flame signifying continuous habit momentum.",
            Tags = new[] { "streak", "fire", "flame", "habit", "hot", "consistency" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M8.5 14.5A2.5 2.5 0 0 0 11 12c0-1.38-.5-2-1-3-1.072-2.143-.224-4.054 2-6 .5 2.5 2 4.9 4 6.5 2 1.6 3 3.5 3 5.5a7 7 0 1 1-14 0c0-1.153.433-2.294 1-3a2.5 2.5 0 0 0 2.5 2.5Z""/></svg>"
        },
        new SvgIconItem
        {
            Key = "trophy-best",
            Name = "Achievement Trophy",
            Category = "Habits & Productivity",
            Description = "Championship trophy for milestone streaks and best records.",
            Tags = new[] { "trophy", "best", "winner", "award", "achievement", "record" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M6 9H4.5a2.5 2.5 0 0 1 0-5H6""/><path d=""M18 9h1.5a2.5 2.5 0 0 0 0-5H18""/><path d=""M4 22h16""/><path d=""M10 14.66V17c0 .55-.45 1-1 1H8v4h8v-4h-1c-.55 0-1-.45-1-1v-2.34""/><path d=""M6 4h12v6c0 3.31-2.69 6-6 6s-6-2.69-6-6V4Z""/></svg>"
        },
        new SvgIconItem
        {
            Key = "target-goal",
            Name = "Target Bullseye",
            Category = "Habits & Productivity",
            Description = "Target with center bullseye for daily habit goals and quotas.",
            Tags = new[] { "target", "goal", "bullseye", "aim", "quota", "objective" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><circle cx=""12"" cy=""12"" r=""10""/><circle cx=""12"" cy=""12"" r=""6""/><circle cx=""12"" cy=""12"" r=""2""/></svg>"
        },
        new SvgIconItem
        {
            Key = "quit-shield",
            Name = "Quit / Abstinence Shield",
            Category = "Habits & Productivity",
            Description = "Protective crest shield with checkmark for addiction recovery and quit habits.",
            Tags = new[] { "shield", "quit", "abstinence", "clean", "sobriety", "relapse" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10Z""/><path d=""m9 12 2 2 4-4""/></svg>"
        },
        new SvgIconItem
        {
            Key = "water-drop",
            Name = "Hydration Drop",
            Category = "Habits & Productivity",
            Description = "Water droplet for quantity tracking (e.g. water intake in ml).",
            Tags = new[] { "water", "drop", "hydration", "fluid", "ml", "quantity" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M12 2.69l5.66 5.66a8 8 0 1 1-11.31 0z""/></svg>"
        },
        new SvgIconItem
        {
            Key = "stopwatch-duration",
            Name = "Duration Stopwatch",
            Category = "Habits & Productivity",
            Description = "Precision stopwatch with pushers for duration and timer-based habits.",
            Tags = new[] { "stopwatch", "duration", "timer", "seconds", "elapsed" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><line x1=""10"" x2=""14"" y1=""2"" y2=""2""/><line x1=""12"" x2=""12"" y1=""14"" y2=""8""/><circle cx=""12"" cy=""14"" r=""8""/></svg>"
        },
        new SvgIconItem
        {
            Key = "star-rating",
            Name = "Star Rating",
            Category = "Habits & Productivity",
            Description = "Five-pointed star for rating and scale habits (1-5, 1-10).",
            Tags = new[] { "star", "rating", "scale", "favorite", "score" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><polygon points=""12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2""/></svg>"
        },
        new SvgIconItem
        {
            Key = "sunrise-time",
            Name = "Sunrise (Time of Day)",
            Category = "Habits & Productivity",
            Description = "Sun ascending over horizon for wake-up time and time-of-day habits.",
            Tags = new[] { "sunrise", "morning", "time", "wake", "schedule", "sun" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M12 2v6""/><path d=""m4.93 10.93 1.41 1.41""/><path d=""M2 18h2""/><path d=""M20 18h2""/><path d=""m19.07 10.93-1.41 1.41""/><path d=""M22 22H2""/><path d=""m8 6 4-4 4 4""/><path d=""M16 18a4 4 0 0 0-8 0""/></svg>"
        },
        new SvgIconItem
        {
            Key = "checklist-items",
            Name = "Checklist Board",
            Category = "Habits & Productivity",
            Description = "Multi-item task checklist with completed ticks.",
            Tags = new[] { "checklist", "subitems", "tasks", "list", "check" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><rect width=""18"" height=""18"" x=""3"" y=""3"" rx=""2""/><path d=""m9 12 2 2 4-4""/><line x1=""9"" x2=""9.01"" y1=""8"" y2=""8""/><line x1=""13"" x2=""15"" y1=""8"" y2=""8""/><line x1=""9"" x2=""9.01"" y1=""16"" y2=""16""/><line x1=""13"" x2=""15"" y1=""16"" y2=""16""/></svg>"
        },
        new SvgIconItem
        {
            Key = "counter-plus",
            Name = "Counter Clicker",
            Category = "Habits & Productivity",
            Description = "Incremental clicker circle with plus symbol for counter habits.",
            Tags = new[] { "counter", "plus", "clicker", "increment", "tally" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><circle cx=""12"" cy=""12"" r=""10""/><line x1=""12"" x2=""12"" y1=""8"" y2=""16""/><line x1=""8"" x2=""16"" y1=""12"" y2=""12""/></svg>"
        },
        new SvgIconItem
        {
            Key = "note-reflection",
            Name = "Journal Note",
            Category = "Habits & Productivity",
            Description = "Notepad document with pen for reflection and note habits.",
            Tags = new[] { "note", "journal", "reflection", "document", "text" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z""/><polyline points=""14 2 14 8 20 8""/><line x1=""16"" x2=""8"" y1=""13"" y2=""13""/><line x1=""16"" x2=""8"" y1=""17"" y2=""17""/><line x1=""10"" x2=""8"" y1=""9"" y2=""9""/></svg>"
        },

        // ─── 3. GYM & FITNESS ───
        new SvgIconItem
        {
            Key = "weight-scale",
            Name = "Body Weight Scale",
            Category = "Gym & Fitness",
            Description = "Bathroom scale with digital display screen for weight tracking.",
            Tags = new[] { "weight", "scale", "body", "kg", "lbs", "bmi", "fitness" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><rect width=""18"" height=""18"" x=""3"" y=""3"" rx=""3""/><rect width=""8"" height=""4"" x=""8"" y=""5"" rx=""1""/><circle cx=""12"" cy=""14"" r=""3""/><line x1=""12"" x2=""13.5"" y1=""14"" y2=""12.5""/></svg>"
        },
        new SvgIconItem
        {
            Key = "dumbbell-gym",
            Name = "Gym Dumbbell",
            Category = "Gym & Fitness",
            Description = "Solid hexagonal fitness dumbbell with knurled grip handle.",
            Tags = new[] { "dumbbell", "weights", "gym", "lift", "training", "exercise" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""m6.5 6.5 11 11""/><path d=""m21 21-1-1""/><path d=""m3 3 1 1""/><path d=""m18 22 4-4""/><path d=""m2 6 4-4""/><path d=""m3 10 7-7""/><path d=""m14 21 7-7""/></svg>"
        },
        new SvgIconItem
        {
            Key = "barbell-heavy",
            Name = "Olympic Barbell",
            Category = "Gym & Fitness",
            Description = "Horizontal barbell with heavy loaded weight plates.",
            Tags = new[] { "barbell", "bench", "squat", "deadlift", "powerlifting" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><line x1=""2"" x2=""22"" y1=""12"" y2=""12""/><rect x=""5"" y=""7"" width=""2"" height=""10"" rx=""0.5""/><rect x=""3"" y=""9"" width=""2"" height=""6"" rx=""0.5""/><rect x=""17"" y=""7"" width=""2"" height=""10"" rx=""0.5""/><rect x=""19"" y=""9"" width=""2"" height=""6"" rx=""0.5""/></svg>"
        },
        new SvgIconItem
        {
            Key = "bmi-gauge",
            Name = "BMI Gauge Speedometer",
            Category = "Gym & Fitness",
            Description = "Speedometer dial indicating healthy, overweight, or obese BMI ranges.",
            Tags = new[] { "bmi", "gauge", "speedometer", "index", "meter", "health" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M12 14v-4""/><path d=""M3.34 19a10 10 0 1 1 17.32 0""/><path d=""m12 14 3-3""/><circle cx=""12"" cy=""14"" r=""2""/></svg>"
        },
        new SvgIconItem
        {
            Key = "runner-motion",
            Name = "Athletic Runner",
            Category = "Gym & Fitness",
            Description = "Dynamic human runner in motion for cardio and workouts.",
            Tags = new[] { "run", "running", "jog", "cardio", "athletics", "motion" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><circle cx=""17"" cy=""4"" r=""2""/><path d=""m15 8-4.5 4.5 2 3.5 4.5 1""/><path d=""M7 13.5 10.5 11l2-4.5-3.5-1.5""/><path d=""m6 21 3-5""/></svg>"
        },
        new SvgIconItem
        {
            Key = "heart-pulse",
            Name = "Heartbeat Cardio Pulse",
            Category = "Gym & Fitness",
            Description = "Anatomical heart icon traversed by ECG pulse line.",
            Tags = new[] { "heart", "pulse", "cardio", "ecg", "health", "bpm" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z""/><path d=""M3.22 12H9.5l1.5-3 2 6 1.5-3h6.28""/></svg>"
        },

        // ─── 4. FINANCE & DEBTS ───
        new SvgIconItem
        {
            Key = "debt-borrowed",
            Name = "Borrowed Loan (Incoming)",
            Category = "Finance & Debts",
            Description = "Hand open receiving coin from above representing borrowed funds.",
            Tags = new[] { "borrow", "loan", "debt", "hand", "coin", "incoming" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M11 15h2a2 2 0 1 0 0-4h-3c-.6 0-1.1.2-1.4.6L3 17""/><path d=""m7 21 1.6-1.4c.3-.4.8-.6 1.4-.6h4c1.1 0 2.1-.4 2.8-1.2l4.6-4.4a2 2 0 0 0-2.8-2.8L15 13""/><circle cx=""12"" cy=""5"" r=""3""/></svg>"
        },
        new SvgIconItem
        {
            Key = "debt-lent",
            Name = "Lent Money (Outgoing)",
            Category = "Finance & Debts",
            Description = "Hand releasing coin outward representing funds lent to others.",
            Tags = new[] { "lent", "lend", "debt", "receivable", "hand", "credit" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M18 11V6a2 2 0 0 0-2-2v0a2 2 0 0 0-2 2v0""/><path d=""M14 10V4a2 2 0 0 0-2-2v0a2 2 0 0 0-2 2v2""/><path d=""M10 10.5V6a2 2 0 0 0-2-2v0a2 2 0 0 0-2 2v8""/><path d=""M18 8a2 2 0 1 1 4 0v6a8 8 0 0 1-8 8h-2c-2.8 0-4.5-.86-5.99-2.34l-3.6-3.6a2 2 0 0 1 2.83-2.82L7 15""/></svg>"
        },
        new SvgIconItem
        {
            Key = "wallet-finance",
            Name = "Leather Wallet",
            Category = "Finance & Debts",
            Description = "Pocket wallet with card slots and security clasp.",
            Tags = new[] { "wallet", "money", "cash", "finance", "savings" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M19 7V4a1 1 0 0 0-1-1H5a2 2 0 0 0 0 4h15a1 1 0 0 1 1 1v4h-3a2 2 0 0 0 0 4h3a1 1 0 0 0 1-1v-2a1 1 0 0 0-1-1""/><path d=""M3 5v14a2 2 0 0 0 2 2h15a1 1 0 0 0 1-1v-4""/></svg>"
        },
        new SvgIconItem
        {
            Key = "credit-card",
            Name = "Credit / Debit Card",
            Category = "Finance & Debts",
            Description = "Smart payment card with EMV microchip.",
            Tags = new[] { "card", "credit", "debit", "payment", "bank" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><rect width=""20"" height=""14"" x=""2"" y=""5"" rx=""2""/><line x1=""2"" x2=""22"" y1=""10"" y2=""10""/></svg>"
        },
        new SvgIconItem
        {
            Key = "pie-chart-finance",
            Name = "Analytics Donut Chart",
            Category = "Finance & Debts",
            Description = "Circular sector chart for debt breakdowns and category distributions.",
            Tags = new[] { "chart", "pie", "analytics", "graph", "breakdown" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M21.21 15.89A10 10 0 1 1 8 2.83""/><path d=""M22 12A10 10 0 0 0 12 2v10z""/></svg>"
        },
        new SvgIconItem
        {
            Key = "trend-growth",
            Name = "Upward Growth Trend",
            Category = "Finance & Debts",
            Description = "Upward ascending line chart indicating growth and positive return.",
            Tags = new[] { "trend", "growth", "up", "profit", "gain" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><polyline points=""22 7 13.5 15.5 8.5 10.5 2 17""/><polyline points=""16 7 22 7 22 13""/></svg>"
        },

        // ─── 5. SYSTEM & UI ───
        new SvgIconItem
        {
            Key = "calendar-grid",
            Name = "Calendar Schedule",
            Category = "System & UI",
            Description = "Monthly calendar grid sheet for events, logs, and dates.",
            Tags = new[] { "calendar", "schedule", "events", "month", "date" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><rect width=""18"" height=""18"" x=""3"" y=""4"" rx=""2""/><line x1=""16"" x2=""16"" y1=""2"" y2=""6""/><line x1=""8"" x2=""8"" y1=""2"" y2=""6""/><line x1=""3"" x2=""21"" y1=""10"" y2=""10""/></svg>"
        },
        new SvgIconItem
        {
            Key = "task-check",
            Name = "Task Completed Circle",
            Category = "System & UI",
            Description = "Circle enclosing clean checkmark for completed tasks and items.",
            Tags = new[] { "check", "task", "done", "complete", "success" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><circle cx=""12"" cy=""12"" r=""10""/><path d=""m9 12 2 2 4-4""/></svg>"
        },
        new SvgIconItem
        {
            Key = "sun-solar",
            Name = "Radiant Sun",
            Category = "System & UI",
            Description = "Sun with radiating beams for Shamsi calendar and daytime theme.",
            Tags = new[] { "sun", "solar", "day", "shamsi", "light" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><circle cx=""12"" cy=""12"" r=""4""/><path d=""M12 2v2""/><path d=""M12 20v2""/><path d=""m4.93 4.93 1.41 1.41""/><path d=""m17.66 17.66 1.41 1.41""/><path d=""M2 12h2""/><path d=""M20 12h2""/><path d=""m6.34 17.66-1.41 1.41""/><path d=""m19.07 4.93-1.41 1.41""/></svg>"
        },
        new SvgIconItem
        {
            Key = "moon-lunar",
            Name = "Crescent Moon",
            Category = "System & UI",
            Description = "Crescent moon for Ghamari lunar calendar and night mode.",
            Tags = new[] { "moon", "lunar", "night", "ghamari", "dark" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z""/></svg>"
        },
        new SvgIconItem
        {
            Key = "birthday-cake",
            Name = "Celebration Cake",
            Category = "System & UI",
            Description = "Birthday cake with candles for age calculator and birthday alerts.",
            Tags = new[] { "birthday", "cake", "candle", "age", "gift", "party" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M20 21v-8a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v8""/><path d=""M4 16s.5-1 2-1 2.5 2 4 2 2.5-2 4-2 2.5 2 4 2 2-1 2-1""/><path d=""M2 21h20""/><path d=""M7 8v3""/><path d=""M12 8v3""/><path d=""M17 8v3""/><circle cx=""7"" cy=""4"" r=""1"" fill=""currentColor""/><circle cx=""12"" cy=""4"" r=""1"" fill=""currentColor""/><circle cx=""17"" cy=""4"" r=""1"" fill=""currentColor""/></svg>"
        },
        new SvgIconItem
        {
            Key = "zodiac-star",
            Name = "Celestial Star / Horoscope",
            Category = "System & UI",
            Description = "Four-pointed sparkle diamond for Zodiac astrology signs.",
            Tags = new[] { "zodiac", "star", "horoscope", "astrology", "sparkle" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M12 2v20""/><path d=""M2 12h20""/><path d=""m4.93 4.93 14.14 14.14""/><path d=""m19.07 4.93-14.14 14.14""/></svg>"
        },
        new SvgIconItem
        {
            Key = "clipboard-copy",
            Name = "Copy to Clipboard",
            Category = "System & UI",
            Description = "Two overlapping documents indicating quick clipboard copy.",
            Tags = new[] { "copy", "clipboard", "duplicate", "clone" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><rect width=""14"" height=""14"" x=""8"" y=""8"" rx=""2""/><path d=""M4 16c-1.1 0-2-.9-2-2V4c0-1.1.9-2 2-2h10c1.1 0 2 .9 2 2""/></svg>"
        },
        new SvgIconItem
        {
            Key = "search-magnifier",
            Name = "Search Magnifier",
            Category = "System & UI",
            Description = "Glass magnifying lens for global search and icon filtering.",
            Tags = new[] { "search", "find", "magnifier", "lookup", "filter" },
            SvgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><circle cx=""11"" cy=""11"" r=""8""/><line x1=""21"" x2=""16.65"" y1=""21"" y2=""16.65""/></svg>"
        }
    };

    public static IReadOnlyList<SvgIconItem> GetAll() => _icons;

    public static IReadOnlyList<string> GetCategories() =>
        _icons.Select(x => x.Category).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    public static IEnumerable<SvgIconItem> Search(string? term, string? category = null)
    {
        var query = _icons.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            var clean = term.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLowerInvariant().Contains(clean) ||
                x.Key.ToLowerInvariant().Contains(clean) ||
                x.Description.ToLowerInvariant().Contains(clean) ||
                x.Tags.Any(t => t.ToLowerInvariant().Contains(clean)));
        }

        return query;
    }
}
