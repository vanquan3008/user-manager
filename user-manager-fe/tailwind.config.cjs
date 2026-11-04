/** Design tokens live in src/styles.scss as RGB triplets so Tailwind opacity modifiers work. */
const token = (name) => `rgb(var(--${name}) / <alpha-value>)`;

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  darkMode: 'class',
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', '-apple-system', 'Segoe UI', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'ui-monospace', 'SFMono-Regular', 'Menlo', 'monospace']
      },
      colors: {
        canvas: token('canvas'),
        surface: { DEFAULT: token('surface'), 2: token('surface-2'), 3: token('surface-3') },
        line: { DEFAULT: token('line'), strong: token('line-strong') },
        fg: { DEFAULT: token('fg'), 2: token('fg-2'), 3: token('fg-3') },
        brand: { DEFAULT: token('brand'), hover: token('brand-hover'), soft: token('brand-soft'), fg: token('brand-fg'), text: token('brand-text') },
        ok: { DEFAULT: token('ok'), soft: token('ok-soft') },
        warn: { DEFAULT: token('warn'), soft: token('warn-soft') },
        bad: { DEFAULT: token('bad'), soft: token('bad-soft') },
        info: { DEFAULT: token('info'), soft: token('info-soft') }
      },
      borderRadius: { xl: '0.75rem', '2xl': '1rem' },
      boxShadow: {
        xs: '0 1px 2px 0 rgb(0 0 0 / 0.04)',
        card: '0 1px 2px 0 rgb(0 0 0 / 0.04), 0 1px 3px 0 rgb(0 0 0 / 0.03)',
        pop: '0 10px 30px -10px rgb(0 0 0 / 0.25), 0 4px 10px -6px rgb(0 0 0 / 0.12)'
      },
      fontSize: { '2xs': ['0.6875rem', { lineHeight: '1rem' }] },
      keyframes: {
        'fade-up': { from: { opacity: '0', transform: 'translateY(4px)' }, to: { opacity: '1', transform: 'none' } },
        'scale-in': { from: { opacity: '0', transform: 'scale(.97)' }, to: { opacity: '1', transform: 'none' } },
        'slide-in': { from: { transform: 'translateX(100%)' }, to: { transform: 'none' } }
      },
      animation: {
        'fade-up': 'fade-up 220ms cubic-bezier(.2,.8,.2,1) both',
        'scale-in': 'scale-in 160ms cubic-bezier(.2,.8,.2,1) both',
        'slide-in': 'slide-in 260ms cubic-bezier(.2,.8,.2,1) both'
      }
    }
  },
  plugins: []
};
