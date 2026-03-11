import React from 'react';

const KpiCard = ({ title, value, subtext, icon, trend, trendValue, color = 'primary' }) => {

    const colorClasses = {
        primary: 'text-blue-500 bg-blue-500/10',
        emerald: 'text-emerald-500 bg-emerald-500/10',
        amber: 'text-amber-500 bg-amber-500/10',
        blue: 'text-sky-500 bg-sky-500/10', // using sky for info
        red: 'text-red-500 bg-red-500/10',
    };

    const activeColorClass = colorClasses[color] || colorClasses.primary;

    return (
        <div className="bg-white rounded-2xl p-6 border border-slate-200 shadow-sm hover:shadow-md transition-shadow duration-200">
            <div className="flex justify-between items-start">
                <div>
                    <p className="text-sm font-medium text-slate-500 mb-1">{title}</p>
                    <h3 className="text-2xl font-bold text-slate-800 font-outfit tracking-tight">{value}</h3>
                </div>
                <div className={`p-3 rounded-xl ${activeColorClass}`}>
                    {icon}
                </div>
            </div>

            {(trend || subtext) && (
                <div className="mt-4 flex items-center text-sm">
                    {trend === 'up' && (
                        <span className="text-emerald-500 font-medium flex items-center bg-emerald-50 px-2 py-0.5 rounded-full text-xs mr-2">
                            <svg className="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" /></svg>
                            {trendValue}
                        </span>
                    )}
                    {trend === 'down' && (
                        <span className="text-red-500 font-medium flex items-center bg-red-50 px-2 py-0.5 rounded-full text-xs mr-2">
                            <svg className="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 17h8m0 0V9m0 8l-8-8-4 4-6-6" /></svg>
                            {trendValue}
                        </span>
                    )}
                    <span className="text-slate-400 text-xs">{subtext}</span>
                </div>
            )}
        </div>
    );
};

export default KpiCard;
