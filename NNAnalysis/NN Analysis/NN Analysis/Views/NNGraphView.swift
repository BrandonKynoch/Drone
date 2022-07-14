//
//  NNGraphView.swift
//  NN Analysis
//
//  Created by Brandon Kynoch on 7/13/22.
//

import SwiftUI

//https://blog.logrocket.com/building-custom-charts-swiftui/

struct NNGraphView: View {
    @State var dataPoints: [Double] = [15, 2, 7, 16, 32, 39, 5, 3, 25, 21]

  var body: some View {
    LineChartView(dataPoints: dataPoints)
      .frame(height: 200)
      .padding(4)
      .background(Color.gray.opacity(0.1).cornerRadius(16))
      .padding()
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
          path.move(to: CGPoint(
            x: CGFloat(index) * width / CGFloat(dataPoints.count - 1),
            y: height * dataPoints[index] / highestPoint))

          path.addArc(center: CGPoint(
            x: CGFloat(index) * width / CGFloat(dataPoints.count - 1),
            y: height * self.ratio(for: index)),
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

struct NNGraphView_Previews: PreviewProvider {
    static var previews: some View {
        NNGraphView()
    }
}
