//
//  LineChartView.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 7/14/22.
//

import SwiftUI

struct LineChartView: View {
    var dataPoints: [Double]
    var lineColor: Color = .red
    var outerCircleColor: Color = .red
    var innerCircleColor: Color = .white
    
    var body: some View {
        ZStack {
            LineView(dataPoints: dataPoints)
                .accentColor(lineColor)
            
            LineChartCircleView(dataPoints: dataPoints, radius: 3.0)
                .accentColor(outerCircleColor)
            
            LineChartCircleView(dataPoints: dataPoints, radius: 1.0)
                .accentColor(innerCircleColor)
        }
    }
}

struct LineView: View {
    var dataPoints: [Double]
    
    var highestPoint: Double {
        let max = dataPoints.max() ?? 1.0
        if max == 0 { return 1.0 }
        return max
    }
    
    var body: some View {
        GeometryReader { geometry in
            let height = geometry.size.height
            let width = geometry.size.width
            
            Path { path in
                path.move(to: CGPoint(x: 0, y: height * self.ratio(for: 0)))
                
                for index in 1..<dataPoints.count {
                    path.addLine(to: CGPoint(
                        x: CGFloat(index) * width / CGFloat(dataPoints.count - 1),
                        y: height * self.ratio(for: index)))
                }
            }
            .stroke(Color.accentColor, style: StrokeStyle(lineWidth: 2, lineJoin: .round))
        }
        .padding(.vertical)
    }
    
    private func ratio(for index: Int) -> Double {
        1 - (dataPoints[index] / highestPoint)
    }
}

struct LineChartCircleView: View {
    var dataPoints: [Double]
    var radius: CGFloat
    
    var highestPoint: Double {
        let max = dataPoints.max() ?? 1.0
        if max == 0 { return 1.0 }
        return max
    }
    
    var body: some View {
        GeometryReader { geometry in
            let height = geometry.size.height
            let width = geometry.size.width
            
            Path { path in
                path.move(to: CGPoint(x: 0, y: (height * self.ratio(for: 0)) - radius))
                
                path.addArc(center: CGPoint(x: 0, y: height * self.ratio(for: 0)),
                            radius: radius, startAngle: .zero,
                            endAngle: .degrees(360.0), clockwise: false)
                
                for index in 1..<dataPoints.count {
                    let circleX = (CGFloat(index) * width / CGFloat(dataPoints.count - 1))
                    let circleY = height - (height * dataPoints[index] / highestPoint)
                    
                    path.move(to: CGPoint(x: circleX, y: circleY))
                    
                    path.addArc(center: CGPoint(x: circleX, y: circleY),
                                radius: radius, startAngle: .zero,
                                endAngle: .degrees(360.0), clockwise: false)
                }
            }
            .stroke(Color.accentColor, lineWidth: 2)
        }
        .padding(.vertical)
    }
    
    private func ratio(for index: Int) -> Double {
        1 - (dataPoints[index] / highestPoint)
    }
}

//struct LineChartView_Previews: PreviewProvider {
//    static var previews: some View {
//        LineChartView()
//    }
//}
