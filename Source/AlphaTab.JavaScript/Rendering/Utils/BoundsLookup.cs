﻿using AlphaTab.Collections;
using AlphaTab.Model;
using AlphaTab.Platform;
using SharpKit.JavaScript;

namespace AlphaTab.Rendering.Utils
{
    partial class BoundsLookup
    {
        public object ToJson()
        {
            var json = Std.NewObject();

            var staveGroups = new FastList<StaveGroupBounds>();
            json.StaveGroups = staveGroups;

            foreach (var group in StaveGroups)
            {
                StaveGroupBounds g = Std.NewObject();
                g.VisualBounds = BoundsToJson(group.VisualBounds);
                g.RealBounds = BoundsToJson(group.RealBounds);
                g.Bars = new FastList<MasterBarBounds>();

                foreach (var masterBar in group.Bars)
                {
                    MasterBarBounds mb = Std.NewObject();
                    mb.VisualBounds = BoundsToJson(masterBar.VisualBounds);
                    mb.RealBounds = BoundsToJson(masterBar.RealBounds);

                    mb.Bars = new FastList<BarBounds>();

                    foreach (var bar in masterBar.Bars)
                    {
                        BarBounds b = Std.NewObject();
                        b.VisualBounds = BoundsToJson(bar.VisualBounds);
                        b.RealBounds = BoundsToJson(bar.RealBounds);

                        b.Beats = new FastList<BeatBounds>();

                        foreach (var beat in bar.Beats)
                        {
                            var bb = Std.NewObject();

                            bb.VisualBounds = BoundsToJson(beat.VisualBounds);
                            bb.RealBounds = BoundsToJson(beat.RealBounds);
                            bb.BeatIndex = beat.Beat.Index;
                            bb.VoiceIndex = beat.Beat.Voice.Index;
                            bb.BarIndex = beat.Beat.Voice.Bar.Index;
                            bb.StaffIndex = beat.Beat.Voice.Bar.Staff.Index;
                            bb.TrackIndex = beat.Beat.Voice.Bar.Staff.Track.Index;

                            b.Beats.Add(bb);
                        }

                        mb.Bars.Add(b);
                    }

                    g.Bars.Add(mb);
                }

                staveGroups.Add(g);
            }

            return json;
        }

        public static BoundsLookup FromJson(object json, Score score)
        {
            var lookup = new BoundsLookup();

            var staveGroups = json.Member("StaveGroups").As<FastList<StaveGroupBounds>>();
            foreach (var staveGroup in staveGroups)
            {
                var sg = new StaveGroupBounds();
                sg.VisualBounds = BoundsFromJson(staveGroup.VisualBounds);
                sg.RealBounds = BoundsFromJson(staveGroup.RealBounds);
                lookup.AddStaveGroup(sg);

                foreach (var masterBar in staveGroup.Bars)
                {
                    var mb = new MasterBarBounds();
                    mb.VisualBounds = BoundsFromJson(masterBar.VisualBounds);
                    mb.RealBounds = BoundsFromJson(masterBar.RealBounds);
                    sg.AddBar(mb);

                    foreach (var bar in masterBar.Bars)
                    {
                        var b = new BarBounds();
                        b.VisualBounds = BoundsFromJson(bar.VisualBounds);
                        b.RealBounds = BoundsFromJson(bar.RealBounds);
                        mb.AddBar(b);

                        foreach (var beat in bar.Beats)
                        {
                            var bb = new BeatBounds();
                            bb.VisualBounds = BoundsFromJson(beat.VisualBounds);
                            bb.RealBounds = BoundsFromJson(beat.RealBounds);
                            bb.Beat = score
                                .Tracks[beat.Member("TrackIndex").As<int>()]
                                .Staves[beat.Member("StaffIndex").As<int>()]
                                .Bars[beat.Member("BarIndex").As<int>()]
                                .Voices[beat.Member("VoiceIndex").As<int>()]
                                .Beats[beat.Member("BeatIndex").As<int>()];

                            b.AddBeat(bb);
                        }
                    }
                }
            }

            return lookup;
        }

        private static Bounds BoundsFromJson(Bounds json)
        {
            return new Bounds(json.X, json.Y, json.W, json.H);
        }

        private Bounds BoundsToJson(Bounds bounds)
        {
            var json = Std.NewObject();
            json.X = bounds.X;
            json.Y = bounds.Y;
            json.W = bounds.W;
            json.H = bounds.H;
            return json;
        }
    }
}
