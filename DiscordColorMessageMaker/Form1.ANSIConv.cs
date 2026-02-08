using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace DiscordColorMessageMaker
{
    public partial class Form1 : Form
    {
        private string BuildDiscordAnsiFromMarks(string content)
        {
            const string ESC = "\u001b";

            if (_marks.Count == 0)
                _marks.Add((0, _state));

            _marks.RemoveAll(m => m.pos < 0 || m.pos > content.Length);
            _marks.Sort((a, b) => a.pos.CompareTo(b.pos));

            var sb = new StringBuilder(content.Length * 2);
            int markIdx = 0;

            AnsiState? current = null; // null = 플레인(아무 스타일 없음)

            bool SameStyle(AnsiState? a, AnsiState? b)
            {
                if (a == null && b == null) return true;
                if (a == null || b == null) return false;
                return a.Value.Bold == b.Value.Bold &&
                       a.Value.Under == b.Value.Under &&
                       a.Value.Fg == b.Value.Fg &&
                       a.Value.Bg == b.Value.Bg;
            }

            for (int i = 0; i <= content.Length; i++)
            {
                // i 위치에 마커가 있다면 즉시 스타일 전환 판단
                while (markIdx < _marks.Count && _marks[markIdx].pos == i)
                {
                    var next = _marks[markIdx].state;

                    if (!SameStyle(current, next))
                    {
                        bool nextHasAnyStyle = next.Bold || next.Under || next.Fg.HasValue || next.Bg.HasValue;

                        if (current is AnsiState cur)
                        {
                            bool curHasAnyStyle = cur.Bold || cur.Under || cur.Fg.HasValue || cur.Bg.HasValue;

                            // 1) 색 → 색 없음(볼드/밑줄만 or 완전 플레인)  또는
                            // 2) 색 일부 제거(전경/배경 중 하나만 해제)  →  하드 리셋 후 재적용
                            bool needHardReset =
                                (HasColor(cur) && !HasColor(next)) ||
                                (HasColor(cur) && LosingColorComponent(cur, next));

                            if (needHardReset)
                            {
                                // 색을 확실히 지움
                                sb.Append(ESC).Append("[0m");
                                // 다음 상태가 플레인이 아니면(볼드/밑줄/색 중 하나라도 있으면) 다시 켬
                                if (nextHasAnyStyle)
                                    sb.Append(ESC).Append('[').Append(CodesOf(next)).Append('m');
                                current = nextHasAnyStyle ? next : (AnsiState?)null;
                            }
                            else
                            {
                                // 플레인으로 가는 단순 해제(스타일만 꺼짐): 0m 한 번
                                if (!nextHasAnyStyle)
                                {
                                    if (curHasAnyStyle)
                                    {
                                        sb.Append(ESC).Append("[0m");
                                        current = null;
                                    }
                                }
                                else
                                {
                                    // 그 외(색↔색, 스타일 변경 포함)는 바로 새 코드
                                    sb.Append(ESC).Append('[').Append(CodesOf(next)).Append('m');
                                    current = next;
                                }
                            }
                        }
                        else
                        {
                            // 현재 플레인 → 무언가 켬
                            if (nextHasAnyStyle)
                            {
                                sb.Append(ESC).Append('[').Append(CodesOf(next)).Append('m');
                                current = next;
                            }
                            // 플레인→플레인 전환은 출력 없음
                        }
                    }
                    markIdx++;
                }

                if (i == content.Length) break;

                char ch = content[i];

                // 줄바꿈: 줄 끝에서 스타일이 켜져 있으면 닫고 줄바꿈 출력
                if (ch == '\r' || ch == '\n')
                {
                    if (current != null)
                    {
                        sb.Append(ESC).Append("[0m");
                        current = null; // 다음 줄은 플레인으로 시작
                    }
                    sb.Append(ch);
                    continue;
                }

                // 일반 문자
                sb.Append(ch);
            }

            // 파일 끝에서 스타일 남아 있으면 닫기
            if (current != null)
                sb.Append(ESC).Append("[0m");

            return $"```ansi\n{sb}\n```";
        }
    }
}
